using MAACO.Agents.Abstractions;
using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Domain.Entities;
using MAACO.Core.Domain.Enums;

namespace MAACO.Agents.Services;

public sealed class AgentExecutionService(
    IAgentRegistry agentRegistry,
    ILogRepository? logRepository = null,
    IArtifactRepository? artifactRepository = null) : IAgentExecutionService
{
    public async Task<AgentResult> ExecuteAsync(
        string agentName,
        AgentContext context,
        CancellationToken cancellationToken)
    {
        var safetyError = AgentInputSafetyGuard.Validate(context);
        if (safetyError is not null)
        {
            return new AgentResult(
                Succeeded: false,
                Output: string.Empty,
                Error: safetyError,
                Metadata: new Dictionary<string, string>
                {
                    ["agent"] = agentName,
                    ["decision"] = "Blocked direct file access payload. Tools layer is required."
                });
        }

        var agent = agentRegistry.GetByName(agentName);
        if (agent is null)
        {
            return new AgentResult(
                Succeeded: false,
                Output: string.Empty,
                Error: $"Agent '{agentName}' is not registered.");
        }

        cancellationToken.ThrowIfCancellationRequested();
        var startedAt = DateTimeOffset.UtcNow;
        var result = await agent.ExecuteAsync(context, cancellationToken);
        var finalizedResult = result with
        {
            Duration = result.Duration ?? (DateTimeOffset.UtcNow - startedAt)
        };

        await PersistExecutionAsync(agentName, context, finalizedResult, cancellationToken);
        return finalizedResult;
    }

    private async Task PersistExecutionAsync(
        string agentName,
        AgentContext context,
        AgentResult result,
        CancellationToken cancellationToken)
    {
        if (logRepository is not null)
        {
            await logRepository.AddAsync(
                new LogEvent
                {
                    WorkflowId = context.WorkflowId,
                    TaskId = context.TaskId,
                    Severity = result.Succeeded ? LogSeverity.Information : LogSeverity.Error,
                    Message = $"Agent {agentName} execution completed. Success: {result.Succeeded}.",
                    CorrelationId = context.CorrelationId
                },
                cancellationToken);

            await logRepository.SaveChangesAsync(cancellationToken);
        }

        if (artifactRepository is not null)
        {
            var metadataText = result.Metadata is null || result.Metadata.Count == 0
                ? string.Empty
                : string.Join(";", result.Metadata.Select(x => $"{x.Key}={x.Value}"));

            await artifactRepository.AddAsync(
                new Artifact
                {
                    TaskId = context.TaskId,
                    Type = ArtifactType.RuntimeLog,
                    Path = $"agent://{agentName}/output",
                    Hash = $"{result.Output} {metadataText}".Trim()
                },
                cancellationToken);

            await artifactRepository.SaveChangesAsync(cancellationToken);
        }
    }
}

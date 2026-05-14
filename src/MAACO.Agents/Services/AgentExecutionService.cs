using MAACO.Agents.Abstractions;

namespace MAACO.Agents.Services;

public sealed class AgentExecutionService(IAgentRegistry agentRegistry) : IAgentExecutionService
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
        return result with
        {
            Duration = result.Duration ?? (DateTimeOffset.UtcNow - startedAt)
        };
    }
}

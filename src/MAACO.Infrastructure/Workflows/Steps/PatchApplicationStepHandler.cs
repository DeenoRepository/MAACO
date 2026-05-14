using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Abstractions.Tools;
using MAACO.Core.Abstractions.Workflows;
using MAACO.Core.Domain.Entities;
using MAACO.Core.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace MAACO.Infrastructure.Workflows.Steps;

public sealed class PatchApplicationStepHandler(
    ILogRepository logRepository,
    IArtifactRepository artifactRepository,
    IServiceProvider serviceProvider) : IWorkflowStepHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public string Name => "PatchApplicationStep";

    public async Task ExecuteAsync(
        WorkflowExecutionContext context,
        WorkflowStep step,
        CancellationToken cancellationToken)
    {
        var patchFilePath = await ResolveLatestPatchFilePathAsync(context, cancellationToken);
        if (string.IsNullOrWhiteSpace(patchFilePath) || !File.Exists(patchFilePath))
        {
            await logRepository.AddAsync(
                new LogEvent
                {
                    WorkflowId = context.WorkflowId,
                    TaskId = context.TaskId,
                    Severity = LogSeverity.Information,
                    CorrelationId = context.CorrelationId,
                    Message = $"Executed {Name} for workflow {context.WorkflowId:D}. Patch not found, skipping apply."
                },
                cancellationToken);
            await logRepository.SaveChangesAsync(cancellationToken);
            return;
        }

        if (context.Inputs is null ||
            !context.Inputs.TryGetValue("WorkspacePath", out var workspacePath) ||
            string.IsNullOrWhiteSpace(workspacePath))
        {
            throw new InvalidOperationException("WorkspacePath is required for patch application.");
        }

        var patchPayload = await File.ReadAllTextAsync(patchFilePath, cancellationToken);
        var patch = JsonSerializer.Deserialize<PatchCandidate>(patchPayload, JsonOptions);
        if (patch is null ||
            string.IsNullOrWhiteSpace(patch.TargetPath) ||
            string.IsNullOrEmpty(patch.OldText))
        {
            throw new InvalidOperationException("Patch payload is invalid.");
        }

        var toolRequest = new ToolRequest(
            ToolName: "CodePatchTool",
            Input: JsonSerializer.Serialize(new
            {
                patch.TargetPath,
                patch.OldText,
                patch.NewText,
                patch.RequireSingleMatch
            }),
            WorkspacePath: workspacePath,
            Permissions: [ToolPermission.WorkspaceWrite],
            Timeout: TimeSpan.FromSeconds(30),
            CorrelationId: context.CorrelationId);

        var toolRegistry = serviceProvider.GetService<IToolRegistry>();
        if (toolRegistry is null)
        {
            await logRepository.AddAsync(
                new LogEvent
                {
                    WorkflowId = context.WorkflowId,
                    TaskId = context.TaskId,
                    Severity = LogSeverity.Warning,
                    CorrelationId = context.CorrelationId,
                    Message = $"Executed {Name} for workflow {context.WorkflowId:D}. Tool registry unavailable, patch apply skipped."
                },
                cancellationToken);
            await logRepository.SaveChangesAsync(cancellationToken);
            return;
        }

        var result = await toolRegistry.ExecuteAsync(toolRequest, cancellationToken);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Patch apply failed: {result.Error ?? "unknown error"}");
        }

        await logRepository.AddAsync(
            new LogEvent
            {
                WorkflowId = context.WorkflowId,
                TaskId = context.TaskId,
                Severity = LogSeverity.Information,
                CorrelationId = context.CorrelationId,
                Message = $"Executed {Name} for workflow {context.WorkflowId:D}. PatchApplied=True."
            },
            cancellationToken);
        await logRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task<string?> ResolveLatestPatchFilePathAsync(
        WorkflowExecutionContext context,
        CancellationToken cancellationToken)
    {
        var patchArtifact = (await artifactRepository.ListByTaskIdAsync(context.TaskId, cancellationToken))
            .Where(x =>
                x.Type == ArtifactType.Patch &&
                x.Hash?.Contains($"workflow={context.WorkflowId:D}", StringComparison.OrdinalIgnoreCase) == true)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefault();

        return patchArtifact?.Path;
    }

    private sealed record PatchCandidate(
        string TargetPath,
        string OldText,
        string NewText,
        bool RequireSingleMatch = true);
}

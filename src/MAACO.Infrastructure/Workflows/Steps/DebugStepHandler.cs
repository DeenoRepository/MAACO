using MAACO.Core.Abstractions.Llm;
using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Abstractions.Workflows;
using MAACO.Core.Domain.Entities;
using MAACO.Core.Domain.Enums;
using System.Text.Json;

namespace MAACO.Infrastructure.Workflows.Steps;

public sealed class DebugStepHandler(
    ILlmGateway llmGateway,
    ILogRepository logRepository,
    IArtifactRepository artifactRepository) : IWorkflowStepHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public string Name => "DebugStep";

    public async Task ExecuteAsync(
        WorkflowExecutionContext context,
        WorkflowStep step,
        CancellationToken cancellationToken)
    {
        var diagnosticsSummary = await BuildDiagnosticsSummaryAsync(context.WorkflowId, cancellationToken);
        var llmResponse = await llmGateway.GenerateAsync(
            new LlmRequest(
                Messages:
                [
                    new LlmMessage(LlmMessageRole.System, "You are MAACO debug step."),
                    new LlmMessage(LlmMessageRole.User, $"Debug workflow {context.WorkflowId:D}. Diagnostics: {diagnosticsSummary}. Return JSON with fields: targetPath, oldText, newText, requireSingleMatch.")
                ],
                TaskType: LlmTaskType.Debugging,
                WorkflowId: context.WorkflowId,
                CorrelationId: context.CorrelationId),
            cancellationToken);

        var patchCandidate = TryParsePatchCandidate(llmResponse.Content) ?? BuildPatchCandidateFromInputs(context);
        var patchSaved = await TryPersistPatchCandidateAsync(context, patchCandidate, cancellationToken);

        await logRepository.AddAsync(
            new LogEvent
            {
                WorkflowId = context.WorkflowId,
                TaskId = context.TaskId,
                Severity = LogSeverity.Information,
                CorrelationId = context.CorrelationId,
                Message = $"Executed {Name}. Provider={llmResponse.Provider}; Model={llmResponse.Model}. DiagnosticsSummaryIncluded={!string.IsNullOrWhiteSpace(diagnosticsSummary)}. PatchPrepared={patchSaved}."
            },
            cancellationToken);
        await logRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task<string> BuildDiagnosticsSummaryAsync(Guid workflowId, CancellationToken cancellationToken)
    {
        var logs = await logRepository.ListByWorkflowIdAsync(workflowId, cancellationToken);
        var summaryLines = logs
            .Where(x => x.Message.StartsWith("Diagnostics summary:", StringComparison.Ordinal))
            .Select(x => x.Message)
            .TakeLast(3)
            .ToList();

        return summaryLines.Count == 0
            ? "none"
            : string.Join(" | ", summaryLines);
    }

    private static PatchCandidate? TryParsePatchCandidate(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        try
        {
            var candidate = JsonSerializer.Deserialize<PatchCandidate>(content, JsonOptions);
            return IsValid(candidate) ? candidate : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static PatchCandidate? BuildPatchCandidateFromInputs(WorkflowExecutionContext context)
    {
        if (context.Inputs is null ||
            !context.Inputs.TryGetValue("DebugPatchTargetPath", out var targetPath) ||
            !context.Inputs.TryGetValue("DebugPatchOldText", out var oldText))
        {
            return null;
        }

        context.Inputs.TryGetValue("DebugPatchNewText", out var newText);
        var requireSingleMatch = true;
        if (context.Inputs.TryGetValue("DebugPatchRequireSingleMatch", out var rawRequireSingleMatch) &&
            bool.TryParse(rawRequireSingleMatch, out var parsed))
        {
            requireSingleMatch = parsed;
        }

        var candidate = new PatchCandidate(targetPath, oldText, newText ?? string.Empty, requireSingleMatch);
        return IsValid(candidate) ? candidate : null;
    }

    private async Task<bool> TryPersistPatchCandidateAsync(
        WorkflowExecutionContext context,
        PatchCandidate? candidate,
        CancellationToken cancellationToken)
    {
        if (!IsValid(candidate) ||
            context.Inputs is null ||
            !context.Inputs.TryGetValue("WorkspacePath", out var workspacePath) ||
            string.IsNullOrWhiteSpace(workspacePath))
        {
            return false;
        }

        var patchesDirectory = Path.Combine(workspacePath, ".maaco", "patches");
        Directory.CreateDirectory(patchesDirectory);

        var patchFilePath = Path.Combine(patchesDirectory, $"debug-{context.WorkflowId:D}.json");
        await File.WriteAllTextAsync(
            patchFilePath,
            JsonSerializer.Serialize(candidate, JsonOptions),
            cancellationToken);

        await artifactRepository.AddAsync(
            new Artifact
            {
                TaskId = context.TaskId,
                Type = ArtifactType.Patch,
                Path = patchFilePath,
                Hash = $"workflow={context.WorkflowId:D};target={candidate!.TargetPath}"
            },
            cancellationToken);
        await artifactRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static bool IsValid(PatchCandidate? candidate) =>
        candidate is not null &&
        !string.IsNullOrWhiteSpace(candidate.TargetPath) &&
        !string.IsNullOrEmpty(candidate.OldText);

    private sealed record PatchCandidate(
        string TargetPath,
        string OldText,
        string NewText,
        bool RequireSingleMatch = true);
}

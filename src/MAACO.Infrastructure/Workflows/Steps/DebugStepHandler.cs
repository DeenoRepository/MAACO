using MAACO.Core.Abstractions.Llm;
using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Abstractions.Workflows;
using MAACO.Core.Domain.Entities;
using MAACO.Core.Domain.Enums;

namespace MAACO.Infrastructure.Workflows.Steps;

public sealed class DebugStepHandler(
    ILlmGateway llmGateway,
    ILogRepository logRepository) : IWorkflowStepHandler
{
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
                    new LlmMessage(LlmMessageRole.User, $"Debug workflow {context.WorkflowId:D}. Diagnostics: {diagnosticsSummary}")
                ],
                TaskType: LlmTaskType.Debugging,
                WorkflowId: context.WorkflowId,
                CorrelationId: context.CorrelationId),
            cancellationToken);

        await logRepository.AddAsync(
            new LogEvent
            {
                WorkflowId = context.WorkflowId,
                TaskId = context.TaskId,
                Severity = LogSeverity.Information,
                CorrelationId = context.CorrelationId,
                Message = $"Executed {Name}. Provider={llmResponse.Provider}; Model={llmResponse.Model}. DiagnosticsSummaryIncluded={!string.IsNullOrWhiteSpace(diagnosticsSummary)}."
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
}

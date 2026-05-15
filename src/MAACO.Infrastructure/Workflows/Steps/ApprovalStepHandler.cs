using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Abstractions.Workflows;
using MAACO.Core.Domain.Entities;
using MAACO.Core.Domain.Enums;

namespace MAACO.Infrastructure.Workflows.Steps;

public sealed class ApprovalStepHandler(
    ILogRepository logRepository,
    IApprovalRepository approvalRepository) : IWorkflowStepHandler
{
    public string Name => "ApprovalStep";

    public async Task ExecuteAsync(
        WorkflowExecutionContext context,
        WorkflowStep step,
        CancellationToken cancellationToken)
    {
        var existingPending = await approvalRepository.GetPendingByWorkflowIdAsync(context.WorkflowId, cancellationToken);
        if (existingPending is null)
        {
            var approvalRequest = new ApprovalRequest
            {
                WorkflowId = context.WorkflowId,
                Status = ApprovalStatus.Pending,
                Mode = ResolveApprovalMode(context),
                RequestedBy = "maaco-system",
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
            };

            await approvalRepository.AddAsync(approvalRequest, cancellationToken);
            await approvalRepository.SaveChangesAsync(cancellationToken);
        }

        await logRepository.AddAsync(
            new LogEvent
            {
                WorkflowId = context.WorkflowId,
                TaskId = context.TaskId,
                Severity = LogSeverity.Information,
                CorrelationId = context.CorrelationId,
                Message = $"Executed {Name} for workflow {context.WorkflowId:D}."
            },
            cancellationToken);
        await logRepository.SaveChangesAsync(cancellationToken);
    }

    private static ApprovalMode ResolveApprovalMode(WorkflowExecutionContext context)
    {
        if (context.Inputs is not null &&
            context.Inputs.TryGetValue("ApprovalMode", out var approvalModeRaw) &&
            Enum.TryParse<ApprovalMode>(approvalModeRaw, ignoreCase: true, out var parsedMode))
        {
            return parsedMode;
        }

        return ApprovalMode.Conditional;
    }
}

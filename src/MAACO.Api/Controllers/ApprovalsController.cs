using MAACO.Api.Contracts.Approvals;
using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Domain.Entities;
using MAACO.Core.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace MAACO.Api.Controllers;

[ApiController]
[Route("api/approvals")]
public sealed class ApprovalsController(IApprovalRepository approvalRepository, ILogRepository logRepository) : ControllerBase
{
    [HttpGet("pending")]
    [ProducesResponseType(typeof(IReadOnlyList<ApprovalDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ApprovalDto>>> GetPendingApprovals(CancellationToken cancellationToken)
    {
        var approvals = await approvalRepository.ListPendingAsync(cancellationToken);
        return Ok(approvals.Select(Map).ToList());
    }

    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(typeof(ApprovalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApprovalDto>> Approve(Guid id, CancellationToken cancellationToken)
    {
        var approval = await approvalRepository.GetByIdAsync(id, cancellationToken);
        if (approval is null)
        {
            return this.NotFoundError("Approval request not found.");
        }

        approval.Status = ApprovalStatus.Approved;
        approval.UpdatedAt = DateTimeOffset.UtcNow;
        await approvalRepository.SaveChangesAsync(cancellationToken);
        await PersistApprovalDecisionAuditAsync(approval, cancellationToken);

        return Ok(Map(approval));
    }

    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(typeof(ApprovalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApprovalDto>> Reject(Guid id, CancellationToken cancellationToken)
    {
        var approval = await approvalRepository.GetByIdAsync(id, cancellationToken);
        if (approval is null)
        {
            return this.NotFoundError("Approval request not found.");
        }

        approval.Status = ApprovalStatus.Rejected;
        approval.UpdatedAt = DateTimeOffset.UtcNow;
        await approvalRepository.SaveChangesAsync(cancellationToken);
        await PersistApprovalDecisionAuditAsync(approval, cancellationToken);

        return Ok(Map(approval));
    }

    private async Task PersistApprovalDecisionAuditAsync(ApprovalRequest approval, CancellationToken cancellationToken)
    {
        var auditEvent = new LogEvent
        {
            WorkflowId = approval.WorkflowId,
            Severity = LogSeverity.Information,
            Message = $"Approval decision: {approval.Status} (ApprovalId={approval.Id})",
            CorrelationId = HttpContext.TraceIdentifier,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await logRepository.AddAsync(auditEvent, cancellationToken);
        await logRepository.SaveChangesAsync(cancellationToken);
    }

    private static ApprovalDto Map(ApprovalRequest approval) =>
        new(
            approval.Id,
            approval.WorkflowId,
            approval.Status.ToString(),
            approval.Mode.ToString(),
            approval.RequestedBy,
            approval.ExpiresAt,
            approval.CreatedAt,
            approval.UpdatedAt,
            approval.Version);
}

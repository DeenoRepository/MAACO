using MAACO.Core.Domain.Entities;

namespace MAACO.Core.Abstractions.Repositories;

public interface IApprovalRepository
{
    Task<ApprovalRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ApprovalRequest?> GetPendingByWorkflowIdAsync(Guid workflowId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ApprovalRequest>> ListPendingAsync(CancellationToken cancellationToken);
    Task AddAsync(ApprovalRequest approvalRequest, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

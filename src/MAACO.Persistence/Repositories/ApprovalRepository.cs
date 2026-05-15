using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Domain.Entities;
using MAACO.Core.Domain.Enums;
using MAACO.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace MAACO.Persistence.Repositories;

public sealed class ApprovalRepository(MaacoDbContext dbContext) : IApprovalRepository
{
    public Task<ApprovalRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.ApprovalRequests.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<ApprovalRequest?> GetPendingByWorkflowIdAsync(Guid workflowId, CancellationToken cancellationToken) =>
        dbContext.ApprovalRequests
            .Where(x => x.WorkflowId == workflowId && x.Status == ApprovalStatus.Pending)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<ApprovalRequest>> ListPendingAsync(CancellationToken cancellationToken) =>
        await dbContext.ApprovalRequests
            .Where(x => x.Status == ApprovalStatus.Pending)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task AddAsync(ApprovalRequest approvalRequest, CancellationToken cancellationToken) =>
        dbContext.ApprovalRequests.AddAsync(approvalRequest, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}

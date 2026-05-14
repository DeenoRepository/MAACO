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

    public async Task<IReadOnlyList<ApprovalRequest>> ListPendingAsync(CancellationToken cancellationToken) =>
        await dbContext.ApprovalRequests
            .Where(x => x.Status == ApprovalStatus.Pending)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}

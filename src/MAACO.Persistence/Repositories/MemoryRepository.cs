using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Domain.Entities;
using MAACO.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace MAACO.Persistence.Repositories;

public sealed class MemoryRepository(MaacoDbContext dbContext) : IMemoryRepository
{
    public Task AddAsync(MemoryRecord memoryRecord, CancellationToken cancellationToken) =>
        dbContext.MemoryRecords.AddAsync(memoryRecord, cancellationToken).AsTask();

    public async Task<IReadOnlyList<MemoryRecord>> ListByProjectIdAsync(Guid projectId, CancellationToken cancellationToken)
    {
        var records = await dbContext.MemoryRecords
            .Where(x => x.ProjectId == projectId)
            .ToListAsync(cancellationToken);

        return records
            .OrderBy(x => x.CreatedAt)
            .ToList();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}

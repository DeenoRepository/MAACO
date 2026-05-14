using MAACO.Core.Domain.Entities;

namespace MAACO.Core.Abstractions.Repositories;

public interface IMemoryRepository
{
    Task AddAsync(MemoryRecord memoryRecord, CancellationToken cancellationToken);
    Task<IReadOnlyList<MemoryRecord>> ListByProjectIdAsync(Guid projectId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

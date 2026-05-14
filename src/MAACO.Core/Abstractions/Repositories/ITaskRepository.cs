using MAACO.Core.Domain.Entities;

namespace MAACO.Core.Abstractions.Repositories;

public interface ITaskRepository
{
    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<TaskItem>> ListAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<TaskItem>> ListByProjectIdAsync(Guid projectId, CancellationToken cancellationToken);
    Task AddAsync(TaskItem taskItem, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

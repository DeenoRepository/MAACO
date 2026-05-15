using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Domain.Entities;
using MAACO.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace MAACO.Persistence.Repositories;

public sealed class TaskRepository(MaacoDbContext dbContext) : ITaskRepository
{
    public Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.TaskItems.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<TaskItem>> ListAsync(CancellationToken cancellationToken)
    {
        // SQLite cannot translate DateTimeOffset ordering in SQL; sort on client side.
        var tasks = await dbContext.TaskItems.ToListAsync(cancellationToken);
        return tasks
            .OrderByDescending(x => x.CreatedAt)
            .ToList();
    }

    public async Task<IReadOnlyList<TaskItem>> ListByProjectIdAsync(Guid projectId, CancellationToken cancellationToken)
    {
        var tasks = await dbContext.TaskItems
            .Where(x => x.ProjectId == projectId)
            .ToListAsync(cancellationToken);

        return tasks
            .OrderBy(x => x.CreatedAt)
            .ToList();
    }

    public Task AddAsync(TaskItem taskItem, CancellationToken cancellationToken) =>
        dbContext.TaskItems.AddAsync(taskItem, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}

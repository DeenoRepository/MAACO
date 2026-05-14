using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Domain.Entities;
using MAACO.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace MAACO.Persistence.Repositories;

public sealed class LogRepository(MaacoDbContext dbContext) : ILogRepository
{
    public Task AddAsync(LogEvent logEvent, CancellationToken cancellationToken) =>
        dbContext.LogEvents.AddAsync(logEvent, cancellationToken).AsTask();

    public async Task<IReadOnlyList<LogEvent>> ListByWorkflowIdAsync(Guid workflowId, CancellationToken cancellationToken) =>
        await dbContext.LogEvents.Where(x => x.WorkflowId == workflowId).OrderBy(x => x.CreatedAt).ToListAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}

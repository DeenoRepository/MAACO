using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Domain.Entities;
using MAACO.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace MAACO.Persistence.Repositories;

public sealed class LogRepository(MaacoDbContext dbContext) : ILogRepository
{
    public Task AddAsync(LogEvent logEvent, CancellationToken cancellationToken) =>
        dbContext.LogEvents.AddAsync(logEvent, cancellationToken).AsTask();

    public async Task<IReadOnlyList<LogEvent>> ListByWorkflowIdAsync(Guid workflowId, CancellationToken cancellationToken)
    {
        var logs = await dbContext.LogEvents
            .Where(x => x.WorkflowId == workflowId)
            .ToListAsync(cancellationToken);

        return logs
            .OrderBy(x => x.CreatedAt)
            .ToList();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}

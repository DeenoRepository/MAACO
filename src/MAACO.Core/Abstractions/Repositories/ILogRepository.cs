using MAACO.Core.Domain.Entities;

namespace MAACO.Core.Abstractions.Repositories;

public interface ILogRepository
{
    Task AddAsync(LogEvent logEvent, CancellationToken cancellationToken);
    Task<IReadOnlyList<LogEvent>> ListByWorkflowIdAsync(Guid workflowId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

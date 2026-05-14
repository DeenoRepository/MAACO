using MAACO.Core.Domain.Entities;

namespace MAACO.Core.Abstractions.Repositories;

public interface IBuildRunRepository
{
    Task AddAsync(BuildRun buildRun, CancellationToken cancellationToken);
    Task<IReadOnlyList<BuildRun>> ListByWorkflowIdAsync(Guid workflowId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

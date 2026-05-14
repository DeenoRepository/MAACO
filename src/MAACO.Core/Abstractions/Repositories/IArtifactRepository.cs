using MAACO.Core.Domain.Entities;

namespace MAACO.Core.Abstractions.Repositories;

public interface IArtifactRepository
{
    Task AddAsync(Artifact artifact, CancellationToken cancellationToken);
    Task<IReadOnlyList<Artifact>> ListByTaskIdAsync(Guid taskId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

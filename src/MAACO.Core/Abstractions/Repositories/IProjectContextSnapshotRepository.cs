using MAACO.Core.Domain.Entities;

namespace MAACO.Core.Abstractions.Repositories;

public interface IProjectContextSnapshotRepository
{
    Task AddAsync(ProjectContextSnapshot snapshot, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

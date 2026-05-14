using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Domain.Entities;
using MAACO.Persistence.Data;

namespace MAACO.Persistence.Repositories;

public sealed class ProjectContextSnapshotRepository(MaacoDbContext dbContext) : IProjectContextSnapshotRepository
{
    public Task AddAsync(ProjectContextSnapshot snapshot, CancellationToken cancellationToken) =>
        dbContext.ProjectContextSnapshots.AddAsync(snapshot, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}

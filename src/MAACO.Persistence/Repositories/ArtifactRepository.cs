using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Domain.Entities;
using MAACO.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace MAACO.Persistence.Repositories;

public sealed class ArtifactRepository(MaacoDbContext dbContext) : IArtifactRepository
{
    public Task AddAsync(Artifact artifact, CancellationToken cancellationToken) =>
        dbContext.Artifacts.AddAsync(artifact, cancellationToken).AsTask();

    public async Task<IReadOnlyList<Artifact>> ListByTaskIdAsync(Guid taskId, CancellationToken cancellationToken) =>
        await dbContext.Artifacts
            .Where(x => x.TaskId == taskId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}

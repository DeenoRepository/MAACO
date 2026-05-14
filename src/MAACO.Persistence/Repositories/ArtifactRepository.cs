using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Domain.Entities;
using MAACO.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace MAACO.Persistence.Repositories;

public sealed class ArtifactRepository(MaacoDbContext dbContext) : IArtifactRepository
{
    public Task AddAsync(Artifact artifact, CancellationToken cancellationToken) =>
        dbContext.Artifacts.AddAsync(artifact, cancellationToken).AsTask();

    public async Task<IReadOnlyList<Artifact>> ListByTaskIdAsync(Guid taskId, CancellationToken cancellationToken)
    {
        var artifacts = await dbContext.Artifacts
            .Where(x => x.TaskId == taskId)
            .ToListAsync(cancellationToken);

        return artifacts
            .OrderBy(x => x.CreatedAt)
            .ToList();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}

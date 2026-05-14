using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Domain.Entities;
using MAACO.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace MAACO.Persistence.Repositories;

public sealed class ArtifactRepository(MaacoDbContext dbContext) : IArtifactRepository
{
    public async Task<IReadOnlyList<Artifact>> ListByTaskIdAsync(Guid taskId, CancellationToken cancellationToken) =>
        await dbContext.Artifacts
            .Where(x => x.TaskId == taskId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
}

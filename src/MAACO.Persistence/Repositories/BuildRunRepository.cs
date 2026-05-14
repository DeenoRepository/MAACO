using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Domain.Entities;
using MAACO.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace MAACO.Persistence.Repositories;

public sealed class BuildRunRepository(MaacoDbContext dbContext) : IBuildRunRepository
{
    public Task AddAsync(BuildRun buildRun, CancellationToken cancellationToken) =>
        dbContext.BuildRuns.AddAsync(buildRun, cancellationToken).AsTask();

    public async Task<IReadOnlyList<BuildRun>> ListByWorkflowIdAsync(Guid workflowId, CancellationToken cancellationToken)
    {
        var runs = await dbContext.BuildRuns
            .Where(x => x.WorkflowId == workflowId)
            .ToListAsync(cancellationToken);

        return runs.OrderBy(x => x.CreatedAt).ToList();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}

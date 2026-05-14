using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Domain.Entities;
using MAACO.Persistence.Data;

namespace MAACO.Persistence.Repositories;

public sealed class ToolExecutionRepository(MaacoDbContext dbContext) : IToolExecutionRepository
{
    public Task AddAsync(ToolExecution toolExecution, CancellationToken cancellationToken) =>
        dbContext.ToolExecutions.AddAsync(toolExecution, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}

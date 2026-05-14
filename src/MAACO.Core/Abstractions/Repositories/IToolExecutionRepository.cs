using MAACO.Core.Domain.Entities;

namespace MAACO.Core.Abstractions.Repositories;

public interface IToolExecutionRepository
{
    Task AddAsync(ToolExecution toolExecution, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

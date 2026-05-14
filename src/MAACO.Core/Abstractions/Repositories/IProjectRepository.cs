using MAACO.Core.Domain.Entities;

namespace MAACO.Core.Abstractions.Repositories;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Project>> ListAsync(CancellationToken cancellationToken);
    Task AddAsync(Project project, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

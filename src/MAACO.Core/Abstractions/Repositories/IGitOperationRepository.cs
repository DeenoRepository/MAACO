using MAACO.Core.Domain.Entities;

namespace MAACO.Core.Abstractions.Repositories;

public interface IGitOperationRepository
{
    Task AddAsync(GitOperation gitOperation, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

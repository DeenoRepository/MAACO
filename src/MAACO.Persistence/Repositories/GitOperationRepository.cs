using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Domain.Entities;
using MAACO.Persistence.Data;

namespace MAACO.Persistence.Repositories;

public sealed class GitOperationRepository(MaacoDbContext dbContext) : IGitOperationRepository
{
    public Task AddAsync(GitOperation gitOperation, CancellationToken cancellationToken) =>
        dbContext.GitOperations.AddAsync(gitOperation, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}

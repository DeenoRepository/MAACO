using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Domain.Entities;
using MAACO.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace MAACO.Persistence.Repositories;

public sealed class ProjectRepository(MaacoDbContext dbContext) : IProjectRepository
{
    public Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Projects.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Project>> ListAsync(CancellationToken cancellationToken) =>
        await dbContext.Projects.OrderBy(x => x.Name).ToListAsync(cancellationToken);

    public Task AddAsync(Project project, CancellationToken cancellationToken) =>
        dbContext.Projects.AddAsync(project, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}

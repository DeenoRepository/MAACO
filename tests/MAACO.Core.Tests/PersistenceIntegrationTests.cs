using MAACO.Core.Domain.Entities;
using MAACO.Core.Domain.ValueObjects;
using MAACO.Persistence.Data;
using MAACO.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MAACO.Core.Tests;

public sealed class PersistenceIntegrationTests
{
    [Fact]
    public async Task DbContext_CanSaveAndReadProject()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<MaacoDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var dbContext = new MaacoDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var project = new Project
        {
            Name = "MAACO Test",
            RepositoryPath = new RepositoryPath("C:\\repo\\maaco-test")
        };

        await dbContext.Projects.AddAsync(project);
        await dbContext.SaveChangesAsync();

        var saved = await dbContext.Projects.SingleAsync();

        Assert.Equal("MAACO Test", saved.Name);
        Assert.Equal("C:\\repo\\maaco-test", saved.RepositoryPath.Value);
    }

    [Fact]
    public async Task ProjectRepository_CanPersistAndListProjects()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<MaacoDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var dbContext = new MaacoDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var repository = new ProjectRepository(dbContext);

        await repository.AddAsync(new Project
        {
            Name = "A",
            RepositoryPath = new RepositoryPath("C:\\repo\\a")
        }, CancellationToken.None);
        await repository.SaveChangesAsync(CancellationToken.None);

        var items = await repository.ListAsync(CancellationToken.None);

        Assert.Single(items);
        Assert.Equal("A", items[0].Name);
    }

    [Fact]
    public async Task DbSeed_SeedsDefaultAgentsAndTools()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<MaacoDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var dbContext = new MaacoDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        await DbSeed.InitializeAsync(dbContext, CancellationToken.None);

        var count = await dbContext.AgentDefinitions.CountAsync();
        var hasPlanner = await dbContext.AgentDefinitions.AnyAsync(x => x.Name == "TaskPlannerAgent");
        var hasTool = await dbContext.AgentDefinitions.AnyAsync(x => x.Name == "GitTool");

        Assert.True(count >= 9);
        Assert.True(hasPlanner);
        Assert.True(hasTool);
    }
}

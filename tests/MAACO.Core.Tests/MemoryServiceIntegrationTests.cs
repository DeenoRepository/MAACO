using MAACO.Core.Abstractions.Memory;
using MAACO.Core.Domain.Entities;
using MAACO.Core.Domain.Enums;
using MAACO.Infrastructure;
using MAACO.Persistence;
using MAACO.Persistence.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MAACO.Core.Tests;

public sealed class MemoryServiceIntegrationTests
{
    [Fact]
    public async Task SaveMethods_PersistAllRequiredMemoryRecords()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var services = new ServiceCollection();
        services.AddMaacoPersistence("Data Source=:memory:");
        services.AddMaacoInfrastructure();
        services.AddDbContext<MaacoDbContext>(options => options.UseSqlite(connection));
        services.AddDbContextFactory<MaacoDbContext>(options => options.UseSqlite(connection));

        await using var provider = services.BuildServiceProvider();
        await using (var initScope = provider.CreateAsyncScope())
        {
            var db = initScope.ServiceProvider.GetRequiredService<MaacoDbContext>();
            await db.Database.EnsureCreatedAsync();
        }

        Guid projectId;
        Guid taskId;

        await using (var seedScope = provider.CreateAsyncScope())
        {
            var db = seedScope.ServiceProvider.GetRequiredService<MaacoDbContext>();
            var project = new Project
            {
                Name = "memory-project",
                RepositoryPath = new MAACO.Core.Domain.ValueObjects.RepositoryPath(".")
            };
            await db.Projects.AddAsync(project);
            await db.SaveChangesAsync();
            projectId = project.Id;

            var task = new TaskItem
            {
                ProjectId = projectId,
                Title = "memory-task"
            };
            await db.TaskItems.AddAsync(task);
            await db.SaveChangesAsync();
            taskId = task.Id;
        }

        await using (var runScope = provider.CreateAsyncScope())
        {
            var memoryService = runScope.ServiceProvider.GetRequiredService<IMemoryService>();

            await memoryService.SaveProjectSummaryAsync(projectId, "project summary", CancellationToken.None);
            await memoryService.SaveTaskSummaryAsync(taskId, "task summary", CancellationToken.None);
            await memoryService.SaveFileSummaryAsync(projectId, "src/file.cs", "file summary", CancellationToken.None);
            await memoryService.SaveBuildFailureAsync(taskId, "build failure", CancellationToken.None);
            await memoryService.SaveDecisionAsync(projectId, "decision summary", CancellationToken.None);
            await memoryService.SaveAgentNoteAsync(taskId, "agent note", CancellationToken.None);
        }

        await using (var verifyScope = provider.CreateAsyncScope())
        {
            var db = verifyScope.ServiceProvider.GetRequiredService<MaacoDbContext>();
            var records = await db.MemoryRecords.Where(x => x.ProjectId == projectId).ToListAsync();

            Assert.Equal(6, records.Count);
            Assert.Contains(records, x => x.Key == "ProjectSummary" && x.Type == MemoryRecordType.Summary);
            Assert.Contains(records, x => x.Key == $"TaskSummary:{taskId:D}" && x.Type == MemoryRecordType.Summary);
            Assert.Contains(records, x => x.Key == "FileSummary:src/file.cs" && x.Type == MemoryRecordType.Summary);
            Assert.Contains(records, x => x.Key == $"BuildFailure:{taskId:D}" && x.Type == MemoryRecordType.Observation);
            Assert.Contains(records, x => x.Key == "Decision" && x.Type == MemoryRecordType.Decision);
            Assert.Contains(records, x => x.Key == $"AgentNote:{taskId:D}" && x.Type == MemoryRecordType.Observation);
        }
    }

    [Fact]
    public async Task ListMethods_ReturnProjectAndTaskScopedMemory()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var services = new ServiceCollection();
        services.AddMaacoPersistence("Data Source=:memory:");
        services.AddMaacoInfrastructure();
        services.AddDbContext<MaacoDbContext>(options => options.UseSqlite(connection));
        services.AddDbContextFactory<MaacoDbContext>(options => options.UseSqlite(connection));

        await using var provider = services.BuildServiceProvider();
        await using (var initScope = provider.CreateAsyncScope())
        {
            var db = initScope.ServiceProvider.GetRequiredService<MaacoDbContext>();
            await db.Database.EnsureCreatedAsync();
        }

        Guid projectId;
        Guid taskId;
        Guid otherTaskId;

        await using (var seedScope = provider.CreateAsyncScope())
        {
            var db = seedScope.ServiceProvider.GetRequiredService<MaacoDbContext>();
            var project = new Project
            {
                Name = "memory-project-retrieval",
                RepositoryPath = new MAACO.Core.Domain.ValueObjects.RepositoryPath(".")
            };
            await db.Projects.AddAsync(project);
            await db.SaveChangesAsync();
            projectId = project.Id;

            var taskA = new TaskItem { ProjectId = projectId, Title = "task-a" };
            var taskB = new TaskItem { ProjectId = projectId, Title = "task-b" };
            await db.TaskItems.AddRangeAsync(taskA, taskB);
            await db.SaveChangesAsync();
            taskId = taskA.Id;
            otherTaskId = taskB.Id;
        }

        await using (var runScope = provider.CreateAsyncScope())
        {
            var memoryService = runScope.ServiceProvider.GetRequiredService<IMemoryService>();

            await memoryService.SaveProjectSummaryAsync(projectId, "project summary", CancellationToken.None);
            await memoryService.SaveDecisionAsync(projectId, "decision summary", CancellationToken.None);
            await memoryService.SaveTaskSummaryAsync(taskId, "task a summary", CancellationToken.None);
            await memoryService.SaveBuildFailureAsync(taskId, "task a build failure", CancellationToken.None);
            await memoryService.SaveAgentNoteAsync(otherTaskId, "task b note", CancellationToken.None);

            var byProject = await memoryService.ListByProjectIdAsync(projectId, CancellationToken.None);
            var byTaskA = await memoryService.ListByTaskIdAsync(taskId, CancellationToken.None);

            Assert.Equal(5, byProject.Count);
            Assert.Equal(2, byTaskA.Count);
            Assert.All(byTaskA, x => Assert.Contains(taskId.ToString("D"), x.Key, StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public async Task SearchByProjectId_FiltersByKeyword_AndLimitsTopN()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var services = new ServiceCollection();
        services.AddMaacoPersistence("Data Source=:memory:");
        services.AddMaacoInfrastructure();
        services.AddDbContext<MaacoDbContext>(options => options.UseSqlite(connection));
        services.AddDbContextFactory<MaacoDbContext>(options => options.UseSqlite(connection));

        await using var provider = services.BuildServiceProvider();
        await using (var initScope = provider.CreateAsyncScope())
        {
            var db = initScope.ServiceProvider.GetRequiredService<MaacoDbContext>();
            await db.Database.EnsureCreatedAsync();
        }

        Guid projectId;
        Guid taskId;

        await using (var seedScope = provider.CreateAsyncScope())
        {
            var db = seedScope.ServiceProvider.GetRequiredService<MaacoDbContext>();
            var project = new Project
            {
                Name = "memory-search-project",
                RepositoryPath = new MAACO.Core.Domain.ValueObjects.RepositoryPath(".")
            };
            await db.Projects.AddAsync(project);
            await db.SaveChangesAsync();
            projectId = project.Id;

            var task = new TaskItem { ProjectId = projectId, Title = "memory-search-task" };
            await db.TaskItems.AddAsync(task);
            await db.SaveChangesAsync();
            taskId = task.Id;
        }

        await using (var runScope = provider.CreateAsyncScope())
        {
            var memoryService = runScope.ServiceProvider.GetRequiredService<IMemoryService>();

            await memoryService.SaveProjectSummaryAsync(projectId, "pipeline summary", CancellationToken.None);
            await memoryService.SaveDecisionAsync(projectId, "select pipeline strategy", CancellationToken.None);
            await memoryService.SaveTaskSummaryAsync(taskId, "pipeline task details", CancellationToken.None);
            await memoryService.SaveFileSummaryAsync(projectId, "src/worker.cs", "worker details", CancellationToken.None);

            var hits = await memoryService.SearchByProjectIdAsync(projectId, "pipeline", 2, CancellationToken.None);

            Assert.Equal(2, hits.Count);
            Assert.All(
                hits,
                x => Assert.True(
                    x.Key.Contains("pipeline", StringComparison.OrdinalIgnoreCase) ||
                    x.Value.Contains("pipeline", StringComparison.OrdinalIgnoreCase)));
        }
    }
}

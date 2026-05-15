using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Abstractions.Workflows;
using MAACO.Core.Domain.Entities;
using MAACO.Core.Domain.Enums;
using MAACO.Infrastructure;
using MAACO.Persistence;
using MAACO.Persistence.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MAACO.Core.Tests;

public sealed class WorkflowStepCheckpointTests
{
    [Fact]
    public async Task ExecuteAsync_PersistsStepInputAndOutputCheckpointLogs()
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

        provider.UseMaacoInfrastructure();
        Guid workflowId;
        Guid taskId;

        await using (var runScope = provider.CreateAsyncScope())
        {
            var db = runScope.ServiceProvider.GetRequiredService<MaacoDbContext>();
            var project = new Project { Name = "checkpoint-project", RepositoryPath = new MAACO.Core.Domain.ValueObjects.RepositoryPath(".") };
            await db.Projects.AddAsync(project);
            await db.SaveChangesAsync();
            var task = new TaskItem { ProjectId = project.Id, Title = "checkpoint-task" };
            await db.TaskItems.AddAsync(task);
            await db.SaveChangesAsync();
            taskId = task.Id;

            var workflowRepository = runScope.ServiceProvider.GetRequiredService<IWorkflowRepository>();
            var workflow = new Workflow { TaskId = taskId, Status = WorkflowStatus.Created };
            await workflowRepository.AddWorkflowAsync(workflow, CancellationToken.None);
            await workflowRepository.SaveChangesAsync(CancellationToken.None);
            workflowId = workflow.Id;

            var orchestrator = runScope.ServiceProvider.GetRequiredService<IWorkflowOrchestrator>();
            await orchestrator.ExecuteAsync(
                new WorkflowExecutionContext(
                    project.Id,
                    taskId,
                    workflowId,
                    "checkpoint-test",
                    "corr-checkpoint",
                    new Dictionary<string, string> { ["WorkspacePath"] = "." }),
                ["ProjectScanStep"],
                CancellationToken.None);
        }

        await using (var verifyScope = provider.CreateAsyncScope())
        {
            var db = verifyScope.ServiceProvider.GetRequiredService<MaacoDbContext>();
            var logs = await db.LogEvents.Where(x => x.WorkflowId == workflowId).ToListAsync();

            Assert.Contains(logs, x => x.Message.Contains("StepCheckpoint input ProjectScanStep#1", StringComparison.Ordinal));
            Assert.Contains(logs, x => x.Message.Contains("StepCheckpoint output ProjectScanStep#1", StringComparison.Ordinal));
            Assert.Contains(logs, x => x.CorrelationId == "corr-checkpoint");
        }
    }
}

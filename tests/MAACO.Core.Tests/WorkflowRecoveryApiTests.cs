using MAACO.Api.Controllers;
using MAACO.Api.Contracts.Workflows;
using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Abstractions.Workflows;
using MAACO.Core.Domain.Entities;
using MAACO.Core.Domain.Enums;
using MAACO.Infrastructure;
using MAACO.Persistence;
using MAACO.Persistence.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MAACO.Core.Tests;

public sealed class WorkflowRecoveryApiTests
{
    [Fact]
    public async Task GetWorkflow_AfterRestart_ReturnsFailedWorkflowWithFailureReason()
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

        await using (var runScope = provider.CreateAsyncScope())
        {
            var db = runScope.ServiceProvider.GetRequiredService<MaacoDbContext>();
            var project = new Project
            {
                Name = "failed-workflow-project",
                RepositoryPath = new MAACO.Core.Domain.ValueObjects.RepositoryPath(".")
            };
            await db.Projects.AddAsync(project);
            await db.SaveChangesAsync();

            var task = new TaskItem
            {
                ProjectId = project.Id,
                Title = "failed-workflow-task"
            };
            await db.TaskItems.AddAsync(task);
            await db.SaveChangesAsync();

            var workflowRepository = runScope.ServiceProvider.GetRequiredService<IWorkflowRepository>();
            var workflow = new Workflow
            {
                TaskId = task.Id,
                Status = WorkflowStatus.Created
            };
            await workflowRepository.AddWorkflowAsync(workflow, CancellationToken.None);
            await workflowRepository.SaveChangesAsync(CancellationToken.None);
            workflowId = workflow.Id;

            var orchestrator = runScope.ServiceProvider.GetRequiredService<IWorkflowOrchestrator>();
            await Assert.ThrowsAnyAsync<Exception>(() =>
                orchestrator.ExecuteAsync(
                    new WorkflowExecutionContext(
                        project.Id,
                        task.Id,
                        workflowId,
                        "failed-workflow-recovery",
                        "corr-failed-recovery",
                        new Dictionary<string, string>
                        {
                            ["BuildFailAttempts"] = "5",
                            ["MaxDebugRetries"] = "3"
                        }),
                    ["BuildStep"],
                    CancellationToken.None));
        }

        // Emulate app restart by opening a new scope and querying via API controller.
        await using (var verifyScope = provider.CreateAsyncScope())
        {
            var workflowRepository = verifyScope.ServiceProvider.GetRequiredService<IWorkflowRepository>();
            var logRepository = verifyScope.ServiceProvider.GetRequiredService<ILogRepository>();
            var artifactRepository = verifyScope.ServiceProvider.GetRequiredService<IArtifactRepository>();
            var taskRepository = verifyScope.ServiceProvider.GetRequiredService<ITaskRepository>();
            var scopeFactory = verifyScope.ServiceProvider.GetRequiredService<IServiceScopeFactory>();
            var controller = new WorkflowsController(
                workflowRepository,
                logRepository,
                artifactRepository,
                taskRepository,
                scopeFactory);

            var response = await controller.GetWorkflow(workflowId, CancellationToken.None);
            var ok = Assert.IsType<OkObjectResult>(response.Result);
            var dto = Assert.IsType<WorkflowDto>(ok.Value);

            Assert.Equal(workflowId, dto.Id);
            Assert.Equal("Failed", dto.Status);
            Assert.False(string.IsNullOrWhiteSpace(dto.FailureReason));
            Assert.Contains("failed", dto.FailureReason!, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task RestartScope_PreservesWorkflowStateAndDiagnosticsArtifacts()
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
            var project = new Project
            {
                Name = "restart-state-project",
                RepositoryPath = new MAACO.Core.Domain.ValueObjects.RepositoryPath(".")
            };
            await db.Projects.AddAsync(project);
            await db.SaveChangesAsync();

            var task = new TaskItem
            {
                ProjectId = project.Id,
                Title = "restart-state-task"
            };
            await db.TaskItems.AddAsync(task);
            await db.SaveChangesAsync();
            taskId = task.Id;

            var workflowRepository = runScope.ServiceProvider.GetRequiredService<IWorkflowRepository>();
            var workflow = new Workflow
            {
                TaskId = task.Id,
                Status = WorkflowStatus.Created
            };
            await workflowRepository.AddWorkflowAsync(workflow, CancellationToken.None);
            await workflowRepository.SaveChangesAsync(CancellationToken.None);
            workflowId = workflow.Id;

            var orchestrator = runScope.ServiceProvider.GetRequiredService<IWorkflowOrchestrator>();
            await Assert.ThrowsAnyAsync<Exception>(() =>
                orchestrator.ExecuteAsync(
                    new WorkflowExecutionContext(
                        project.Id,
                        task.Id,
                        workflowId,
                        "restart-state-check",
                        "corr-restart-state",
                        new Dictionary<string, string>
                        {
                            ["BuildFailAttempts"] = "5",
                            ["MaxDebugRetries"] = "3"
                        }),
                    ["BuildStep", "TestStep"],
                    CancellationToken.None));
        }

        // Emulate restart.
        await using (var verifyScope = provider.CreateAsyncScope())
        {
            var db = verifyScope.ServiceProvider.GetRequiredService<MaacoDbContext>();
            var workflow = await db.Workflows.SingleAsync(x => x.Id == workflowId);
            var steps = await db.WorkflowSteps.Where(x => x.WorkflowId == workflowId).ToListAsync();
            var logs = await db.LogEvents.Where(x => x.WorkflowId == workflowId).ToListAsync();

            Assert.Equal(WorkflowStatus.Failed, workflow.Status);
            Assert.NotEmpty(steps);
            Assert.Contains(steps, x => x.Status == WorkflowStepStatus.Failed);
            Assert.NotEmpty(logs);
            Assert.Contains(logs, x => x.Severity == LogSeverity.Error);
        }
    }
}

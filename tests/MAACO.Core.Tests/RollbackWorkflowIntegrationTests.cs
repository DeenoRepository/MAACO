using FluentValidation;
using MAACO.Api.Controllers;
using MAACO.Api.Contracts.Tasks;
using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Domain.Entities;
using MAACO.Core.Domain.Enums;
using MAACO.Persistence;
using MAACO.Persistence.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using DomainTaskStatus = MAACO.Core.Domain.Enums.TaskStatus;

namespace MAACO.Core.Tests;

public sealed class RollbackWorkflowIntegrationTests
{
    [Fact]
    public async Task RollbackTask_WhenRejected_MarksRolledBackAndPersistsRollbackArtifact()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var services = new ServiceCollection();
        services.AddMaacoPersistence("Data Source=:memory:");
        services.AddDbContext<MaacoDbContext>(options => options.UseSqlite(connection));
        services.AddDbContextFactory<MaacoDbContext>(options => options.UseSqlite(connection));

        await using var provider = services.BuildServiceProvider();
        await using (var initScope = provider.CreateAsyncScope())
        {
            var db = initScope.ServiceProvider.GetRequiredService<MaacoDbContext>();
            await db.Database.EnsureCreatedAsync();
        }

        Guid taskId;
        Guid workflowId;
        await using (var runScope = provider.CreateAsyncScope())
        {
            var db = runScope.ServiceProvider.GetRequiredService<MaacoDbContext>();
            var project = new Project
            {
                Name = "rollback-project",
                RepositoryPath = new MAACO.Core.Domain.ValueObjects.RepositoryPath(".")
            };
            await db.Projects.AddAsync(project);
            await db.SaveChangesAsync();

            var task = new TaskItem
            {
                ProjectId = project.Id,
                Title = "rollback-task",
                Status = DomainTaskStatus.WaitingForApproval
            };
            await db.TaskItems.AddAsync(task);
            await db.SaveChangesAsync();
            taskId = task.Id;

            var workflow = new Workflow
            {
                TaskId = task.Id,
                Status = WorkflowStatus.WaitingForApproval
            };
            await db.Workflows.AddAsync(workflow);
            await db.SaveChangesAsync();
            workflowId = workflow.Id;

            await db.WorkflowSteps.AddRangeAsync(
                new WorkflowStep
                {
                    WorkflowId = workflow.Id,
                    Name = "CommitStep",
                    Status = WorkflowStepStatus.Pending,
                    Order = 1
                },
                new WorkflowStep
                {
                    WorkflowId = workflow.Id,
                    Name = "RollbackStep",
                    Status = WorkflowStepStatus.Pending,
                    Order = 2
                });
            await db.SaveChangesAsync();
        }

        await using (var actionScope = provider.CreateAsyncScope())
        {
            var taskRepository = actionScope.ServiceProvider.GetRequiredService<ITaskRepository>();
            var projectRepository = actionScope.ServiceProvider.GetRequiredService<IProjectRepository>();
            var workflowRepository = actionScope.ServiceProvider.GetRequiredService<IWorkflowRepository>();
            var artifactRepository = actionScope.ServiceProvider.GetRequiredService<IArtifactRepository>();
            var logRepository = actionScope.ServiceProvider.GetRequiredService<ILogRepository>();
            var validator = new Mock<IValidator<CreateTaskRequest>>().Object;

            var controller = new TasksController(
                taskRepository,
                projectRepository,
                workflowRepository,
                artifactRepository,
                logRepository,
                validator);

            var response = await controller.RollbackTask(
                taskId,
                new TasksController.RejectTaskRequest("manual reject"),
                CancellationToken.None);

            Assert.NotNull(response.Result);
        }

        await using (var verifyScope = provider.CreateAsyncScope())
        {
            var db = verifyScope.ServiceProvider.GetRequiredService<MaacoDbContext>();
            var task = await db.TaskItems.SingleAsync(x => x.Id == taskId);
            var workflow = await db.Workflows.SingleAsync(x => x.Id == workflowId);
            var steps = await db.WorkflowSteps.Where(x => x.WorkflowId == workflowId).ToListAsync();
            var artifacts = await db.Artifacts.Where(x => x.TaskId == taskId).ToListAsync();
            var logs = await db.LogEvents.Where(x => x.WorkflowId == workflowId).ToListAsync();
            var rollbackArtifact = artifacts.FirstOrDefault(x => x.Path.StartsWith("rollback://task/", StringComparison.Ordinal));
            var rollbackLog = logs.FirstOrDefault(x => x.Message.Contains("Rollback executed", StringComparison.Ordinal));

            Assert.Equal(DomainTaskStatus.RolledBack, task.Status);
            Assert.Equal(WorkflowStatus.RolledBack, workflow.Status);
            Assert.Contains(steps, x => x.Name == "RollbackStep" && x.Status == WorkflowStepStatus.Completed);
            Assert.Contains(steps, x => x.Name == "CommitStep" && x.Status == WorkflowStepStatus.Cancelled);
            Assert.NotNull(rollbackArtifact);
            Assert.NotNull(rollbackLog);
        }
    }
}

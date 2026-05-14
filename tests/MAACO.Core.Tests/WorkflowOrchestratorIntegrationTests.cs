using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Abstractions.Events;
using MAACO.Core.Abstractions.Workflows;
using MAACO.Core.Domain.Entities;
using MAACO.Core.Domain.Enums;
using MAACO.Core.Domain.Events;
using MAACO.Infrastructure;
using MAACO.Persistence;
using MAACO.Persistence.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MAACO.Core.Tests;

public sealed class WorkflowOrchestratorIntegrationTests
{
    [Fact]
    public async Task ExecuteAsync_CreatesWorkflowAndCompletesSteps()
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

        var taskId = Guid.NewGuid();
        Guid workflowId;

        await using (var runScope = provider.CreateAsyncScope())
        {
            var db = runScope.ServiceProvider.GetRequiredService<MaacoDbContext>();
            var project = new Project
            {
                Name = "test-project",
                RepositoryPath = new MAACO.Core.Domain.ValueObjects.RepositoryPath(".")
            };
            await db.Projects.AddAsync(project);
            await db.SaveChangesAsync();
            var task = new TaskItem
            {
                ProjectId = project.Id,
                Title = "task",
                Status = MAACO.Core.Domain.Enums.TaskStatus.Created
            };
            await db.TaskItems.AddAsync(task);
            await db.SaveChangesAsync();
            taskId = task.Id;

            var workflowRepository = runScope.ServiceProvider.GetRequiredService<IWorkflowRepository>();
            var workflow = new Workflow
            {
                TaskId = taskId,
                Status = WorkflowStatus.Created
            };

            await workflowRepository.AddWorkflowAsync(workflow, CancellationToken.None);
            await workflowRepository.SaveChangesAsync(CancellationToken.None);
            workflowId = workflow.Id;

            var orchestrator = runScope.ServiceProvider.GetRequiredService<IWorkflowOrchestrator>();

            await orchestrator.ExecuteAsync(
                new WorkflowExecutionContext(
                    ProjectId: Guid.NewGuid(),
                    TaskId: taskId,
                    WorkflowId: workflowId,
                    Trigger: "integration-test",
                    CorrelationId: "corr-m11"),
                ["ProjectScanStep", "PlanningStep", "CodeGenerationStep", "PatchApplicationStep", "TestGenerationStep", "BuildStep", "TestStep", "DebugStep", "DiffStep", "ApprovalStep", "CommitStep", "RollbackStep", "DocumentationStep", "FinalReportStep"],
                CancellationToken.None);
        }

        await using (var verifyScope = provider.CreateAsyncScope())
        {
            var db = verifyScope.ServiceProvider.GetRequiredService<MaacoDbContext>();
            var workflow = await db.Workflows.SingleAsync(x => x.Id == workflowId);
            var steps = (await db.WorkflowSteps.Where(x => x.WorkflowId == workflowId).ToListAsync())
                .OrderBy(x => x.Order)
                .ToList();
            var artifacts = (await db.Artifacts.Where(x => x.TaskId == taskId).ToListAsync())
                .OrderBy(x => x.CreatedAt)
                .ToList();
            var logs = await db.LogEvents.Where(x => x.WorkflowId == workflowId).ToListAsync();

            Assert.Equal(WorkflowStatus.Completed, workflow.Status);
            Assert.Equal(14, steps.Count);
            Assert.All(steps, step => Assert.Equal(WorkflowStepStatus.Completed, step.Status));
            Assert.Equal(14, artifacts.Count);
            Assert.All(artifacts, artifact => Assert.Equal(ArtifactType.Snapshot, artifact.Type));
            Assert.Contains(logs, x => x.Message.Contains("Executed ProjectScanStep", StringComparison.Ordinal));
            Assert.Contains(logs, x => x.Message.Contains("Executed PlanningStep", StringComparison.Ordinal));
            Assert.Contains(logs, x => x.Message.Contains("Executed CodeGenerationStep", StringComparison.Ordinal));
            Assert.Contains(logs, x => x.Message.Contains("Executed PatchApplicationStep", StringComparison.Ordinal));
            Assert.Contains(logs, x => x.Message.Contains("Executed TestGenerationStep", StringComparison.Ordinal));
            Assert.Contains(logs, x => x.Message.Contains("Executed BuildStep", StringComparison.Ordinal));
            Assert.Contains(logs, x => x.Message.Contains("Executed TestStep", StringComparison.Ordinal));
            Assert.Contains(logs, x => x.Message.Contains("Executed DebugStep", StringComparison.Ordinal));
            Assert.Contains(logs, x => x.Message.Contains("Executed DiffStep", StringComparison.Ordinal));
            Assert.Contains(logs, x => x.Message.Contains("Executed ApprovalStep", StringComparison.Ordinal));
            Assert.Contains(logs, x => x.Message.Contains("Executed CommitStep", StringComparison.Ordinal));
            Assert.Contains(logs, x => x.Message.Contains("Executed RollbackStep", StringComparison.Ordinal));
            Assert.Contains(logs, x => x.Message.Contains("Executed DocumentationStep", StringComparison.Ordinal));
            Assert.Contains(logs, x => x.Message.Contains("Executed FinalReportStep", StringComparison.Ordinal));
        }
    }

    [Fact]
    public async Task ExecuteAsync_PublishesRealtimeStepEvents()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var collector = new StepEventCollector();
        var services = new ServiceCollection();
        services.AddMaacoPersistence("Data Source=:memory:");
        services.AddMaacoInfrastructure();
        services.AddDbContext<MaacoDbContext>(options => options.UseSqlite(connection));
        services.AddDbContextFactory<MaacoDbContext>(options => options.UseSqlite(connection));
        services.AddSingleton(collector);
        services.AddSingleton<IEventHandler<WorkflowStepStartedEvent>>(sp => sp.GetRequiredService<StepEventCollector>());
        services.AddSingleton<IEventHandler<WorkflowStepCompletedEvent>>(sp => sp.GetRequiredService<StepEventCollector>());

        await using var provider = services.BuildServiceProvider();
        await using (var initScope = provider.CreateAsyncScope())
        {
            var db = initScope.ServiceProvider.GetRequiredService<MaacoDbContext>();
            await db.Database.EnsureCreatedAsync();
        }

        provider.UseMaacoInfrastructure();

        await using (var runScope = provider.CreateAsyncScope())
        {
            var db = runScope.ServiceProvider.GetRequiredService<MaacoDbContext>();
            var project = new Project
            {
                Name = "test-project-rt",
                RepositoryPath = new MAACO.Core.Domain.ValueObjects.RepositoryPath(".")
            };
            await db.Projects.AddAsync(project);
            await db.SaveChangesAsync();
            var task = new TaskItem
            {
                ProjectId = project.Id,
                Title = "task-rt",
                Status = MAACO.Core.Domain.Enums.TaskStatus.Created
            };
            await db.TaskItems.AddAsync(task);
            await db.SaveChangesAsync();

            var workflowRepository = runScope.ServiceProvider.GetRequiredService<IWorkflowRepository>();
            var workflow = new Workflow { TaskId = task.Id, Status = WorkflowStatus.Created };
            await workflowRepository.AddWorkflowAsync(workflow, CancellationToken.None);
            await workflowRepository.SaveChangesAsync(CancellationToken.None);

            var orchestrator = runScope.ServiceProvider.GetRequiredService<IWorkflowOrchestrator>();
            await orchestrator.ExecuteAsync(
                new WorkflowExecutionContext(Guid.NewGuid(), workflow.TaskId, workflow.Id, "events-test", "corr-rt"),
                ["Scan", "Plan"],
                CancellationToken.None);
        }

        Assert.Equal(2, collector.StartedCount);
        Assert.Equal(2, collector.CompletedCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancelled_PersistsCancelledWorkflowAndSteps()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        using var cts = new CancellationTokenSource();
        var cancelHandler = new CancelOnFirstStepStartedHandler(cts);

        var services = new ServiceCollection();
        services.AddMaacoPersistence("Data Source=:memory:");
        services.AddMaacoInfrastructure();
        services.AddDbContext<MaacoDbContext>(options => options.UseSqlite(connection));
        services.AddDbContextFactory<MaacoDbContext>(options => options.UseSqlite(connection));
        services.AddSingleton<IEventHandler<WorkflowStepStartedEvent>>(cancelHandler);

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
                Name = "test-project-cancel",
                RepositoryPath = new MAACO.Core.Domain.ValueObjects.RepositoryPath(".")
            };
            await db.Projects.AddAsync(project);
            await db.SaveChangesAsync();
            var task = new TaskItem
            {
                ProjectId = project.Id,
                Title = "task-cancel",
                Status = MAACO.Core.Domain.Enums.TaskStatus.Created
            };
            await db.TaskItems.AddAsync(task);
            await db.SaveChangesAsync();

            var workflowRepository = runScope.ServiceProvider.GetRequiredService<IWorkflowRepository>();
            var workflow = new Workflow { TaskId = task.Id, Status = WorkflowStatus.Created };
            await workflowRepository.AddWorkflowAsync(workflow, CancellationToken.None);
            await workflowRepository.SaveChangesAsync(CancellationToken.None);
            workflowId = workflow.Id;

            var orchestrator = runScope.ServiceProvider.GetRequiredService<IWorkflowOrchestrator>();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                orchestrator.ExecuteAsync(
                    new WorkflowExecutionContext(Guid.NewGuid(), workflow.TaskId, workflow.Id, "cancel-test", "corr-cancel"),
                    ["Scan", "Plan", "Build"],
                    cts.Token));
        }

        await using (var verifyScope = provider.CreateAsyncScope())
        {
            var db = verifyScope.ServiceProvider.GetRequiredService<MaacoDbContext>();
            var workflow = await db.Workflows.SingleAsync(x => x.Id == workflowId);
            var steps = await db.WorkflowSteps.Where(x => x.WorkflowId == workflowId).ToListAsync();

            Assert.Equal(WorkflowStatus.Cancelled, workflow.Status);
            Assert.NotEmpty(steps);
            Assert.Contains(steps, step => step.Status == WorkflowStepStatus.Cancelled);
            Assert.Contains(steps, step => step.Status == WorkflowStepStatus.Completed);
        }
    }

    [Fact]
    public async Task ExecuteAsync_WhenBuildFails_TriggersDebugLoopAndRecovers()
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
            var project = new Project { Name = "retry-project", RepositoryPath = new MAACO.Core.Domain.ValueObjects.RepositoryPath(".") };
            await db.Projects.AddAsync(project);
            await db.SaveChangesAsync();
            var task = new TaskItem { ProjectId = project.Id, Title = "retry-task" };
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
                    Guid.NewGuid(),
                    taskId,
                    workflowId,
                    "retry-build",
                    "corr-retry-build",
                    new Dictionary<string, string>
                    {
                        ["BuildFailAttempts"] = "1",
                        ["MaxDebugRetries"] = "3"
                    }),
                ["BuildStep", "TestStep"],
                CancellationToken.None);
        }

        await using (var verifyScope = provider.CreateAsyncScope())
        {
            var db = verifyScope.ServiceProvider.GetRequiredService<MaacoDbContext>();
            var workflow = await db.Workflows.SingleAsync(x => x.Id == workflowId);
            var logs = await db.LogEvents.Where(x => x.WorkflowId == workflowId).ToListAsync();

            Assert.Equal(WorkflowStatus.Completed, workflow.Status);
            Assert.Equal(1, workflow.RetryCount);
            Assert.Contains(logs, x => x.Message.Contains("Debug attempt 1/3 for BuildStep", StringComparison.Ordinal));
            Assert.Contains(logs, x => x.Message.Contains("Executed DebugStep", StringComparison.Ordinal));
            Assert.Contains(logs, x => x.Message.Contains("Executed PatchApplicationStep", StringComparison.Ordinal));
            Assert.Contains(logs, x => x.Message.Contains("BuildStep recovered on debug attempt 1", StringComparison.Ordinal));
        }
    }

    [Fact]
    public async Task ExecuteAsync_WhenBuildFailsBeyondRetryLimit_MarksWorkflowFailed()
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
            var project = new Project { Name = "retry-fail-project", RepositoryPath = new MAACO.Core.Domain.ValueObjects.RepositoryPath(".") };
            await db.Projects.AddAsync(project);
            await db.SaveChangesAsync();
            var task = new TaskItem { ProjectId = project.Id, Title = "retry-fail-task" };
            await db.TaskItems.AddAsync(task);
            await db.SaveChangesAsync();
            taskId = task.Id;

            var workflowRepository = runScope.ServiceProvider.GetRequiredService<IWorkflowRepository>();
            var workflow = new Workflow { TaskId = taskId, Status = WorkflowStatus.Created };
            await workflowRepository.AddWorkflowAsync(workflow, CancellationToken.None);
            await workflowRepository.SaveChangesAsync(CancellationToken.None);
            workflowId = workflow.Id;

            var orchestrator = runScope.ServiceProvider.GetRequiredService<IWorkflowOrchestrator>();
            await Assert.ThrowsAnyAsync<Exception>(() =>
                orchestrator.ExecuteAsync(
                    new WorkflowExecutionContext(
                        Guid.NewGuid(),
                        taskId,
                        workflowId,
                        "retry-fail-build",
                        "corr-retry-fail",
                        new Dictionary<string, string>
                        {
                            ["BuildFailAttempts"] = "5",
                            ["MaxDebugRetries"] = "3"
                        }),
                    ["BuildStep"],
                    CancellationToken.None));
        }

        await using (var verifyScope = provider.CreateAsyncScope())
        {
            var db = verifyScope.ServiceProvider.GetRequiredService<MaacoDbContext>();
            var workflow = await db.Workflows.SingleAsync(x => x.Id == workflowId);
            var step = await db.WorkflowSteps.SingleAsync(x => x.WorkflowId == workflowId);
            var logs = await db.LogEvents.Where(x => x.WorkflowId == workflowId).ToListAsync();

            Assert.Equal(WorkflowStatus.Failed, workflow.Status);
            Assert.Equal(3, workflow.RetryCount);
            Assert.Equal(WorkflowStepStatus.Failed, step.Status);
            Assert.Contains(logs, x => x.Message.Contains("BuildStep reached max debug retries (3)", StringComparison.Ordinal));
            Assert.Contains(logs, x => x.Message.Contains("failed", StringComparison.OrdinalIgnoreCase));
        }
    }

    private sealed class StepEventCollector :
        IEventHandler<WorkflowStepStartedEvent>,
        IEventHandler<WorkflowStepCompletedEvent>
    {
        public int StartedCount { get; private set; }
        public int CompletedCount { get; private set; }

        public Task HandleAsync(WorkflowStepStartedEvent @event, CancellationToken cancellationToken)
        {
            StartedCount++;
            return Task.CompletedTask;
        }

        public Task HandleAsync(WorkflowStepCompletedEvent @event, CancellationToken cancellationToken)
        {
            CompletedCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class CancelOnFirstStepStartedHandler(CancellationTokenSource cancellationTokenSource) : IEventHandler<WorkflowStepStartedEvent>
    {
        private bool cancelled;

        public Task HandleAsync(WorkflowStepStartedEvent @event, CancellationToken cancellationToken)
        {
            if (!cancelled)
            {
                cancelled = true;
                cancellationTokenSource.Cancel();
            }

            return Task.CompletedTask;
        }
    }
}

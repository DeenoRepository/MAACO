using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Abstractions.Tools;
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
using System.Text.Json;

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

            Assert.Equal(WorkflowStatus.WaitingForApproval, workflow.Status);
            Assert.Equal(14, steps.Count);
            Assert.All(steps.Where(step => step.Order <= 10), step => Assert.Equal(WorkflowStepStatus.Completed, step.Status));
            Assert.All(steps.Where(step => step.Order > 10), step => Assert.Equal(WorkflowStepStatus.Pending, step.Status));
            var orderedNames = steps.OrderBy(x => x.Order).Select(x => x.Name).ToList();
            Assert.Equal(
                [
                    "ProjectScanStep",
                    "PlanningStep",
                    "CodeGenerationStep",
                    "PatchApplicationStep",
                    "TestGenerationStep",
                    "BuildStep",
                    "TestStep",
                    "DebugStep",
                    "DiffStep",
                    "ApprovalStep",
                    "CommitStep",
                    "RollbackStep",
                    "DocumentationStep",
                    "FinalReportStep"
                ],
                orderedNames);
            Assert.Equal(10, artifacts.Count);
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
            Assert.DoesNotContain(logs, x => x.Message.Contains("Executed CommitStep", StringComparison.Ordinal));
            Assert.DoesNotContain(logs, x => x.Message.Contains("Executed RollbackStep", StringComparison.Ordinal));
            Assert.DoesNotContain(logs, x => x.Message.Contains("Executed DocumentationStep", StringComparison.Ordinal));
            Assert.DoesNotContain(logs, x => x.Message.Contains("Executed FinalReportStep", StringComparison.Ordinal));
        }
    }

    [Fact]
    public async Task ExecuteAsync_WhenWorkflowMissing_CreatesWorkflowFromTask()
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

        Guid taskId;
        Guid unknownWorkflowId = Guid.NewGuid();

        await using (var runScope = provider.CreateAsyncScope())
        {
            var db = runScope.ServiceProvider.GetRequiredService<MaacoDbContext>();
            var project = new Project
            {
                Name = "create-from-task-project",
                RepositoryPath = new MAACO.Core.Domain.ValueObjects.RepositoryPath(".")
            };
            await db.Projects.AddAsync(project);
            await db.SaveChangesAsync();

            var task = new TaskItem
            {
                ProjectId = project.Id,
                Title = "create-workflow-from-task"
            };
            await db.TaskItems.AddAsync(task);
            await db.SaveChangesAsync();
            taskId = task.Id;

            var orchestrator = runScope.ServiceProvider.GetRequiredService<IWorkflowOrchestrator>();
            await orchestrator.ExecuteAsync(
                new WorkflowExecutionContext(
                    project.Id,
                    taskId,
                    unknownWorkflowId,
                    "task-created",
                    "corr-create-from-task"),
                ["ProjectScanStep", "PlanningStep"],
                CancellationToken.None);
        }

        await using (var verifyScope = provider.CreateAsyncScope())
        {
            var db = verifyScope.ServiceProvider.GetRequiredService<MaacoDbContext>();
            var workflows = await db.Workflows.Where(x => x.TaskId == taskId).ToListAsync();
            Assert.Single(workflows);
            Assert.Equal(WorkflowStatus.Completed, workflows[0].Status);

            var steps = await db.WorkflowSteps.Where(x => x.WorkflowId == workflows[0].Id).OrderBy(x => x.Order).ToListAsync();
            Assert.Equal(2, steps.Count);
            Assert.Equal("ProjectScanStep", steps[0].Name);
            Assert.Equal("PlanningStep", steps[1].Name);
            Assert.All(steps, x => Assert.Equal(WorkflowStepStatus.Completed, x.Status));
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
            Assert.DoesNotContain(steps, step => step.Status == WorkflowStepStatus.Running);
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
            Assert.Contains(logs, x => x.Message.Contains("DiagnosticsSummaryIncluded=True", StringComparison.Ordinal));
            Assert.Contains(logs, x => x.Message.Contains("Executed PatchApplicationStep", StringComparison.Ordinal));
            Assert.Contains(logs, x => x.Message.Contains("BuildStep recovered on debug attempt 1", StringComparison.Ordinal));
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithDetectedBuildCommand_PersistsBuildRunAndArtifacts()
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
            var project = new Project { Name = "detected-build-project", RepositoryPath = new MAACO.Core.Domain.ValueObjects.RepositoryPath(".") };
            await db.Projects.AddAsync(project);
            await db.SaveChangesAsync();
            var task = new TaskItem { ProjectId = project.Id, Title = "detected-build-task" };
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
                    "detected-build",
                    "corr-build-detected",
                    new Dictionary<string, string>
                    {
                        ["BuildCommand"] = "dotnet --version",
                        ["WorkspacePath"] = Environment.CurrentDirectory
                    }),
                ["BuildStep"],
                CancellationToken.None);
        }

        await using (var verifyScope = provider.CreateAsyncScope())
        {
            var db = verifyScope.ServiceProvider.GetRequiredService<MaacoDbContext>();
            var buildRuns = await db.BuildRuns.Where(x => x.WorkflowId == workflowId).ToListAsync();
            var buildArtifacts = await db.Artifacts.Where(x => x.TaskId == taskId && x.Type == ArtifactType.BuildLog).ToListAsync();
            var logs = await db.LogEvents.Where(x => x.WorkflowId == workflowId).ToListAsync();

            Assert.Single(buildRuns);
            Assert.Equal(BuildRunStatus.Succeeded, buildRuns[0].Status);
            Assert.True(buildRuns[0].Duration >= TimeSpan.Zero);
            Assert.Equal(2, buildArtifacts.Count);
            Assert.Contains(buildArtifacts, x => x.Path.EndsWith("/stdout", StringComparison.Ordinal));
            Assert.Contains(buildArtifacts, x => x.Path.EndsWith("/stderr", StringComparison.Ordinal));
            Assert.Contains(logs, x => x.Message.Contains("ExitCode=0", StringComparison.Ordinal));
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithDetectedTestCommand_PersistsTestArtifactsAndFailedTestCount()
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
            var project = new Project { Name = "detected-test-project", RepositoryPath = new MAACO.Core.Domain.ValueObjects.RepositoryPath(".") };
            await db.Projects.AddAsync(project);
            await db.SaveChangesAsync();
            var task = new TaskItem { ProjectId = project.Id, Title = "detected-test-task" };
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
                    "detected-test",
                    "corr-test-detected",
                    new Dictionary<string, string>
                    {
                        ["TestCommand"] = "dotnet --version",
                        ["WorkspacePath"] = Environment.CurrentDirectory
                    }),
                ["TestStep"],
                CancellationToken.None);
        }

        await using (var verifyScope = provider.CreateAsyncScope())
        {
            var db = verifyScope.ServiceProvider.GetRequiredService<MaacoDbContext>();
            var testArtifacts = await db.Artifacts.Where(x => x.TaskId == taskId && x.Type == ArtifactType.TestLog).ToListAsync();
            var logs = await db.LogEvents.Where(x => x.WorkflowId == workflowId).ToListAsync();

            Assert.Equal(3, testArtifacts.Count);
            Assert.Contains(testArtifacts, x => x.Path.EndsWith("/stdout", StringComparison.Ordinal));
            Assert.Contains(testArtifacts, x => x.Path.EndsWith("/stderr", StringComparison.Ordinal));
            Assert.Contains(testArtifacts, x => x.Path.EndsWith("/failed-tests", StringComparison.Ordinal));
            Assert.Contains(logs, x => x.Message.Contains("Diagnostics summary:", StringComparison.Ordinal));
            Assert.Contains(logs, x => x.Message.Contains("ExitCode=0", StringComparison.Ordinal));
            Assert.Contains(logs, x => x.Message.Contains("FailedTests=0", StringComparison.Ordinal));
        }
    }

    [Fact]
    public async Task ExecuteAsync_PersistsStepStateAfterEachStep()
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
            var project = new Project { Name = "state-project", RepositoryPath = new MAACO.Core.Domain.ValueObjects.RepositoryPath(".") };
            await db.Projects.AddAsync(project);
            await db.SaveChangesAsync();

            var task = new TaskItem { ProjectId = project.Id, Title = "state-task" };
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
                new WorkflowExecutionContext(Guid.NewGuid(), taskId, workflowId, "state-check", "corr-state"),
                ["ProjectScanStep", "PlanningStep", "BuildStep", "TestStep"],
                CancellationToken.None);
        }

        await using (var verifyScope = provider.CreateAsyncScope())
        {
            var db = verifyScope.ServiceProvider.GetRequiredService<MaacoDbContext>();
            var steps = await db.WorkflowSteps.Where(x => x.WorkflowId == workflowId).OrderBy(x => x.Order).ToListAsync();
            var artifacts = (await db.Artifacts.Where(x => x.TaskId == taskId).ToListAsync())
                .OrderBy(x => x.CreatedAt)
                .ToList();

            Assert.Equal(4, steps.Count);
            Assert.All(steps, x => Assert.Equal(WorkflowStepStatus.Completed, x.Status));
            Assert.Equal(4, artifacts.Count);

            for (var i = 0; i < steps.Count; i++)
            {
                var step = steps[i];
                var artifact = artifacts[i];
                Assert.Equal(ArtifactType.Snapshot, artifact.Type);
                Assert.Contains($"/step/{step.Order}", artifact.Path, StringComparison.Ordinal);
                Assert.Contains(step.Name, artifact.Hash ?? string.Empty, StringComparison.Ordinal);
                Assert.Contains("Completed", artifact.Hash ?? string.Empty, StringComparison.Ordinal);
            }
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
            Assert.Contains(logs, x => x.Message.Contains("StepCheckpoint error BuildStep#1", StringComparison.Ordinal));
            Assert.Contains(logs, x => x.Message.Contains("failed", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(logs, x => x.Message.Contains("Debug attempt", StringComparison.Ordinal));
            Assert.Contains(logs, x => x.TaskId == taskId && x.CorrelationId == "corr-retry-fail");
        }
    }

    [Fact]
    public async Task ExecuteAsync_WhenTestsFail_TriggersDebugLoopAndRecovers()
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
            var project = new Project { Name = "retry-test-project", RepositoryPath = new MAACO.Core.Domain.ValueObjects.RepositoryPath(".") };
            await db.Projects.AddAsync(project);
            await db.SaveChangesAsync();
            var task = new TaskItem { ProjectId = project.Id, Title = "retry-test-task" };
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
                    "retry-test",
                    "corr-retry-test",
                    new Dictionary<string, string>
                    {
                        ["TestFailAttempts"] = "1",
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
            Assert.Contains(logs, x => x.Message.Contains("Debug attempt 1/3 for TestStep", StringComparison.Ordinal));
            Assert.Contains(logs, x => x.Message.Contains("TestStep recovered on debug attempt 1", StringComparison.Ordinal));
            Assert.Contains(logs, x => x.Message.Contains("Post-debug validation replayed: BuildStep, TestStep", StringComparison.Ordinal));
        }
    }

    [Fact]
    public async Task ExecuteAsync_AfterDebugRecovery_ContinuesToDiffStep()
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
            var project = new Project { Name = "diff-after-recovery-project", RepositoryPath = new MAACO.Core.Domain.ValueObjects.RepositoryPath(".") };
            await db.Projects.AddAsync(project);
            await db.SaveChangesAsync();
            var task = new TaskItem { ProjectId = project.Id, Title = "diff-after-recovery-task" };
            await db.TaskItems.AddAsync(task);
            await db.SaveChangesAsync();

            var workflowRepository = runScope.ServiceProvider.GetRequiredService<IWorkflowRepository>();
            var workflow = new Workflow { TaskId = task.Id, Status = WorkflowStatus.Created };
            await workflowRepository.AddWorkflowAsync(workflow, CancellationToken.None);
            await workflowRepository.SaveChangesAsync(CancellationToken.None);
            workflowId = workflow.Id;

            var orchestrator = runScope.ServiceProvider.GetRequiredService<IWorkflowOrchestrator>();
            await orchestrator.ExecuteAsync(
                new WorkflowExecutionContext(
                    Guid.NewGuid(),
                    task.Id,
                    workflowId,
                    "diff-after-recovery",
                    "corr-diff-after-recovery",
                    new Dictionary<string, string>
                    {
                        ["BuildFailAttempts"] = "1",
                        ["MaxDebugRetries"] = "3"
                    }),
                ["BuildStep", "TestStep", "DiffStep"],
                CancellationToken.None);
        }

        await using (var verifyScope = provider.CreateAsyncScope())
        {
            var db = verifyScope.ServiceProvider.GetRequiredService<MaacoDbContext>();
            var workflow = await db.Workflows.SingleAsync(x => x.Id == workflowId);
            var steps = await db.WorkflowSteps.Where(x => x.WorkflowId == workflowId).OrderBy(x => x.Order).ToListAsync();
            var logs = await db.LogEvents.Where(x => x.WorkflowId == workflowId).ToListAsync();

            Assert.Equal(WorkflowStatus.Completed, workflow.Status);
            Assert.All(steps, x => Assert.Equal(WorkflowStepStatus.Completed, x.Status));
            Assert.Contains(logs, x => x.Message.Contains("BuildStep recovered on debug attempt 1", StringComparison.Ordinal));
            Assert.Contains(logs, x => x.Message.Contains("Executed DiffStep", StringComparison.Ordinal));
        }
    }

    [Fact]
    public async Task ExecuteAsync_DebugAndPatchSteps_ApplyPatchFromPreparedDebugCandidate()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var workspacePath = Path.Combine(Path.GetTempPath(), $"maaco-debug-patch-{Guid.NewGuid():N}");
        Directory.CreateDirectory(workspacePath);
        var targetFilePath = Path.Combine(workspacePath, "sample.txt");
        await File.WriteAllTextAsync(targetFilePath, "before", CancellationToken.None);

        try
        {
            var services = new ServiceCollection();
            services.AddMaacoPersistence("Data Source=:memory:");
            services.AddMaacoInfrastructure();
            services.AddSingleton<IToolRegistry, FakePatchToolRegistry>();
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
                var project = new Project { Name = "debug-patch-project", RepositoryPath = new MAACO.Core.Domain.ValueObjects.RepositoryPath(workspacePath) };
                await db.Projects.AddAsync(project);
                await db.SaveChangesAsync();
                var task = new TaskItem { ProjectId = project.Id, Title = "debug-patch-task" };
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
                        "debug-patch",
                        "corr-debug-patch",
                        new Dictionary<string, string>
                        {
                            ["WorkspacePath"] = workspacePath,
                            ["DebugPatchTargetPath"] = "sample.txt",
                            ["DebugPatchOldText"] = "before",
                            ["DebugPatchNewText"] = "after"
                        }),
                    ["DebugStep", "PatchApplicationStep"],
                    CancellationToken.None);
            }

            await using (var verifyScope = provider.CreateAsyncScope())
            {
                var db = verifyScope.ServiceProvider.GetRequiredService<MaacoDbContext>();
                var artifacts = await db.Artifacts.Where(x => x.TaskId == taskId && x.Type == ArtifactType.Patch).ToListAsync();
                var logs = await db.LogEvents.Where(x => x.WorkflowId == workflowId).ToListAsync();

                Assert.NotEmpty(artifacts);
                Assert.Contains(logs, x => x.Message.Contains("PatchPrepared=True", StringComparison.Ordinal));
                Assert.Contains(logs, x => x.Message.Contains("PatchApplied=True", StringComparison.Ordinal));
            }

            var fileContent = await File.ReadAllTextAsync(targetFilePath, CancellationToken.None);
            Assert.Equal("after", fileContent);
        }
        finally
        {
            if (Directory.Exists(workspacePath))
            {
                Directory.Delete(workspacePath, recursive: true);
            }
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

    private sealed class FakePatchToolRegistry : IToolRegistry
    {
        public IReadOnlyCollection<string> ListToolNames() => ["CodePatchTool"];

        public async Task<ToolResult> ExecuteAsync(ToolRequest request, CancellationToken cancellationToken)
        {
            if (!string.Equals(request.ToolName, "CodePatchTool", StringComparison.Ordinal))
            {
                return new ToolResult(false, string.Empty, "Unsupported tool.", TimeSpan.Zero, CorrelationId: request.CorrelationId);
            }

            using var document = JsonDocument.Parse(request.Input);
            var root = document.RootElement;
            var targetPath = root.GetProperty("TargetPath").GetString() ?? string.Empty;
            var oldText = root.GetProperty("OldText").GetString() ?? string.Empty;
            var newText = root.GetProperty("NewText").GetString() ?? string.Empty;

            var fullPath = Path.GetFullPath(Path.Combine(request.WorkspacePath, targetPath));
            if (!File.Exists(fullPath))
            {
                return new ToolResult(false, string.Empty, "Target file does not exist.", TimeSpan.Zero, CorrelationId: request.CorrelationId);
            }

            var content = await File.ReadAllTextAsync(fullPath, cancellationToken);
            if (!content.Contains(oldText, StringComparison.Ordinal))
            {
                return new ToolResult(false, string.Empty, "Old text not found.", TimeSpan.Zero, CorrelationId: request.CorrelationId);
            }

            var updated = content.Replace(oldText, newText, StringComparison.Ordinal);
            await File.WriteAllTextAsync(fullPath, updated, cancellationToken);
            return new ToolResult(true, "{\"applied\":true}", null, TimeSpan.Zero, CorrelationId: request.CorrelationId);
        }
    }
}

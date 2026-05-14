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
                ["ProjectScanStep", "PlanningStep"],
                CancellationToken.None);
        }

        await using (var verifyScope = provider.CreateAsyncScope())
        {
            var db = verifyScope.ServiceProvider.GetRequiredService<MaacoDbContext>();
            var workflow = await db.Workflows.SingleAsync(x => x.Id == workflowId);
            var steps = await db.WorkflowSteps.Where(x => x.WorkflowId == workflowId).OrderBy(x => x.Order).ToListAsync();

            Assert.Equal(WorkflowStatus.Completed, workflow.Status);
            Assert.Equal(2, steps.Count);
            Assert.All(steps, step => Assert.Equal(WorkflowStepStatus.Completed, step.Status));
        }
    }
}

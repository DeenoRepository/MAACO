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

public sealed class ApprovalRecoveryIntegrationTests
{
    [Fact]
    public async Task ExecuteAsync_WhenWaitingForApproval_PersistsPendingApprovalRequest()
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
                Name = "approval-recovery-project",
                RepositoryPath = new MAACO.Core.Domain.ValueObjects.RepositoryPath(".")
            };
            await db.Projects.AddAsync(project);
            await db.SaveChangesAsync();

            var task = new TaskItem
            {
                ProjectId = project.Id,
                Title = "approval-recovery-task"
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
            await orchestrator.ExecuteAsync(
                new WorkflowExecutionContext(
                    project.Id,
                    task.Id,
                    workflowId,
                    "approval-recovery-test",
                    "corr-approval-recovery",
                    new Dictionary<string, string> { ["ApprovalMode"] = "Conditional" }),
                ["ApprovalStep"],
                CancellationToken.None);
        }

        await using (var verifyScope = provider.CreateAsyncScope())
        {
            var db = verifyScope.ServiceProvider.GetRequiredService<MaacoDbContext>();
            var workflow = await db.Workflows.SingleAsync(x => x.Id == workflowId);
            var approvals = await db.ApprovalRequests.Where(x => x.WorkflowId == workflowId).ToListAsync();

            Assert.Equal(WorkflowStatus.WaitingForApproval, workflow.Status);
            Assert.Single(approvals);
            Assert.Equal(ApprovalStatus.Pending, approvals[0].Status);
            Assert.Equal(ApprovalMode.Conditional, approvals[0].Mode);
            Assert.Equal("maaco-system", approvals[0].RequestedBy);
        }
    }
}

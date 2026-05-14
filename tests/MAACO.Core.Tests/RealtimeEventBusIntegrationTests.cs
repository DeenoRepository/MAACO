using MAACO.Core.Abstractions.Events;
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

public sealed class RealtimeEventBusIntegrationTests
{
    [Fact]
    public async Task EventBus_TestClientHandler_ReceivesWorkflowStartedEvent()
    {
        var services = new ServiceCollection();
        services.AddMaacoInfrastructure();
        services.AddSingleton<TestWorkflowStartedClientHandler>();
        services.AddSingleton<IEventHandler<WorkflowStartedEvent>>(sp => sp.GetRequiredService<TestWorkflowStartedClientHandler>());

        await using var provider = services.BuildServiceProvider();
        provider.UseMaacoInfrastructure();

        var bus = provider.GetRequiredService<IEventBus>();
        var client = provider.GetRequiredService<TestWorkflowStartedClientHandler>();

        var workflowId = Guid.NewGuid();
        await bus.PublishAsync(new WorkflowStartedEvent(workflowId, Guid.NewGuid(), DateTimeOffset.UtcNow, "corr-test"), CancellationToken.None);

        Assert.True(client.Received);
        Assert.Equal(workflowId, client.LastWorkflowId);
    }

    [Fact]
    public async Task WorkflowStartedEvent_PersistsLogAndUpdatesWorkflowStatus()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var services = new ServiceCollection();
        services.AddMaacoPersistence("Data Source=:memory:");
        services.AddMaacoInfrastructure();

        services.AddDbContext<MaacoDbContext>(options => options.UseSqlite(connection));
        services.AddDbContextFactory<MaacoDbContext>(options => options.UseSqlite(connection));

        await using var provider = services.BuildServiceProvider();

        await using (var scope = provider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MaacoDbContext>();
            await db.Database.EnsureCreatedAsync();

            var workflow = new Workflow
            {
                TaskId = Guid.NewGuid(),
                Status = WorkflowStatus.Created
            };

            await db.Workflows.AddAsync(workflow);
            await db.SaveChangesAsync();
        }

        provider.UseMaacoInfrastructure();
        var bus = provider.GetRequiredService<IEventBus>();

        Guid persistedWorkflowId;
        const string correlationId = "corr-123";

        await using (var scope = provider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MaacoDbContext>();
            persistedWorkflowId = await db.Workflows.Select(x => x.Id).SingleAsync();
        }

        await bus.PublishAsync(
            new WorkflowStartedEvent(
                persistedWorkflowId,
                Guid.NewGuid(),
                DateTimeOffset.UtcNow,
                correlationId),
            CancellationToken.None);

        await using (var verifyScope = provider.CreateAsyncScope())
        {
            var db = verifyScope.ServiceProvider.GetRequiredService<MaacoDbContext>();
            var workflow = await db.Workflows.SingleAsync(x => x.Id == persistedWorkflowId);
            var log = await db.LogEvents.OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync(x => x.WorkflowId == persistedWorkflowId);

            Assert.Equal(WorkflowStatus.Running, workflow.Status);
            Assert.NotNull(log);
            Assert.Equal(correlationId, log!.CorrelationId);
        }
    }

    private sealed class TestWorkflowStartedClientHandler : IEventHandler<WorkflowStartedEvent>
    {
        public bool Received { get; private set; }
        public Guid LastWorkflowId { get; private set; }

        public Task HandleAsync(WorkflowStartedEvent @event, CancellationToken cancellationToken)
        {
            Received = true;
            LastWorkflowId = @event.WorkflowId;
            return Task.CompletedTask;
        }
    }
}

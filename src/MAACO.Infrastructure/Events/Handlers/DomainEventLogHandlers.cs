using MAACO.Core.Abstractions.Events;
using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Domain.Entities;
using MAACO.Core.Domain.Enums;
using MAACO.Core.Domain.Events;
using Microsoft.Extensions.DependencyInjection;

namespace MAACO.Infrastructure.Events.Handlers;

public sealed class TaskCreatedEventLogHandler(IServiceScopeFactory scopeFactory) : IEventHandler<TaskCreatedEvent>
{
    public Task HandleAsync(TaskCreatedEvent @event, CancellationToken cancellationToken) => DomainEventLogHandlerHelpers.PersistLogAsync(
            scopeFactory,
            new LogEvent
            {
                TaskId = @event.TaskId,
                Severity = LogSeverity.Information,
                Message = $"Task created: {@event.TaskId}",
                CorrelationId = @event.CorrelationId
            },
            cancellationToken);
}

public sealed class WorkflowStartedEventLogHandler(IServiceScopeFactory scopeFactory) : IEventHandler<WorkflowStartedEvent>
{
    public Task HandleAsync(WorkflowStartedEvent @event, CancellationToken cancellationToken) => DomainEventLogHandlerHelpers.PersistLogAsync(
            scopeFactory,
            new LogEvent
            {
                WorkflowId = @event.WorkflowId,
                TaskId = @event.TaskId,
                Severity = LogSeverity.Information,
                Message = $"Workflow started: {@event.WorkflowId}",
                CorrelationId = @event.CorrelationId
            },
            cancellationToken);
}

public sealed class WorkflowStepStartedEventLogHandler(IServiceScopeFactory scopeFactory) : IEventHandler<WorkflowStepStartedEvent>
{
    public Task HandleAsync(WorkflowStepStartedEvent @event, CancellationToken cancellationToken) => DomainEventLogHandlerHelpers.PersistLogAsync(
            scopeFactory,
            new LogEvent
            {
                WorkflowId = @event.WorkflowId,
                Severity = LogSeverity.Information,
                Message = $"Workflow step started: {@event.WorkflowStepId}",
                CorrelationId = @event.CorrelationId
            },
            cancellationToken);
}

public sealed class WorkflowStepCompletedEventLogHandler(IServiceScopeFactory scopeFactory) : IEventHandler<WorkflowStepCompletedEvent>
{
    public Task HandleAsync(WorkflowStepCompletedEvent @event, CancellationToken cancellationToken) => DomainEventLogHandlerHelpers.PersistLogAsync(
            scopeFactory,
            new LogEvent
            {
                WorkflowId = @event.WorkflowId,
                Severity = LogSeverity.Information,
                Message = $"Workflow step completed: {@event.WorkflowStepId}",
                CorrelationId = @event.CorrelationId
            },
            cancellationToken);
}

public sealed class WorkflowStepFailedEventLogHandler(IServiceScopeFactory scopeFactory) : IEventHandler<WorkflowStepFailedEvent>
{
    public Task HandleAsync(WorkflowStepFailedEvent @event, CancellationToken cancellationToken) => DomainEventLogHandlerHelpers.PersistLogAsync(
            scopeFactory,
            new LogEvent
            {
                WorkflowId = @event.WorkflowId,
                Severity = LogSeverity.Error,
                Message = $"Workflow step failed: {@event.WorkflowStepId}. Reason: {@event.Reason}",
                CorrelationId = @event.CorrelationId
            },
            cancellationToken);
}

public sealed class ApprovalRequestedEventLogHandler(IServiceScopeFactory scopeFactory) : IEventHandler<ApprovalRequestedEvent>
{
    public Task HandleAsync(ApprovalRequestedEvent @event, CancellationToken cancellationToken) => DomainEventLogHandlerHelpers.PersistLogAsync(
            scopeFactory,
            new LogEvent
            {
                WorkflowId = @event.WorkflowId,
                Severity = LogSeverity.Information,
                Message = $"Approval requested: {@event.ApprovalRequestId}",
                CorrelationId = @event.CorrelationId
            },
            cancellationToken);
}

public sealed class WorkflowCompletedEventLogHandler(IServiceScopeFactory scopeFactory) : IEventHandler<WorkflowCompletedEvent>
{
    public Task HandleAsync(WorkflowCompletedEvent @event, CancellationToken cancellationToken) => DomainEventLogHandlerHelpers.PersistLogAsync(
            scopeFactory,
            new LogEvent
            {
                WorkflowId = @event.WorkflowId,
                Severity = LogSeverity.Information,
                Message = $"Workflow completed: {@event.WorkflowId}",
                CorrelationId = @event.CorrelationId
            },
            cancellationToken);
}

public sealed class WorkflowFailedEventLogHandler(IServiceScopeFactory scopeFactory) : IEventHandler<WorkflowFailedEvent>
{
    public Task HandleAsync(WorkflowFailedEvent @event, CancellationToken cancellationToken) => DomainEventLogHandlerHelpers.PersistLogAsync(
            scopeFactory,
            new LogEvent
            {
                WorkflowId = @event.WorkflowId,
                Severity = LogSeverity.Error,
                Message = $"Workflow failed: {@event.WorkflowId}. Reason: {@event.Reason}",
                CorrelationId = @event.CorrelationId
            },
            cancellationToken);
}

internal static class DomainEventLogHandlerHelpers
{
    public static async Task PersistLogAsync(
        IServiceScopeFactory scopeFactory,
        LogEvent logEvent,
        CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var logRepository = scope.ServiceProvider.GetRequiredService<ILogRepository>();
        await logRepository.AddAsync(logEvent, cancellationToken);
        await logRepository.SaveChangesAsync(cancellationToken);
    }
}


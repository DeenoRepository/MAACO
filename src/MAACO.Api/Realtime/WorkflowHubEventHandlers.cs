using MAACO.Core.Abstractions.Events;
using MAACO.Core.Domain.Events;
using Microsoft.AspNetCore.SignalR;

namespace MAACO.Api.Realtime;

public sealed class TaskCreatedSignalrHandler(IHubContext<WorkflowHub> hubContext) : IEventHandler<TaskCreatedEvent>
{
    public Task HandleAsync(TaskCreatedEvent @event, CancellationToken cancellationToken) =>
        hubContext.Clients.Group(WorkflowHub.ProjectGroup(@event.ProjectId)).SendAsync(
            "TaskCreated",
            new
            {
                @event.TaskId,
                @event.ProjectId,
                @event.OccurredAt,
                @event.CorrelationId
            },
            cancellationToken);
}

public sealed class WorkflowStartedSignalrHandler(IHubContext<WorkflowHub> hubContext) : IEventHandler<WorkflowStartedEvent>
{
    public Task HandleAsync(WorkflowStartedEvent @event, CancellationToken cancellationToken) =>
        hubContext.Clients.Group(WorkflowHub.WorkflowGroup(@event.WorkflowId)).SendAsync(
            "WorkflowStarted",
            new
            {
                @event.WorkflowId,
                @event.TaskId,
                @event.OccurredAt,
                @event.CorrelationId
            },
            cancellationToken);
}

public sealed class StepStartedSignalrHandler(IHubContext<WorkflowHub> hubContext) : IEventHandler<WorkflowStepStartedEvent>
{
    public Task HandleAsync(WorkflowStepStartedEvent @event, CancellationToken cancellationToken) =>
        hubContext.Clients.Group(WorkflowHub.WorkflowGroup(@event.WorkflowId)).SendAsync(
            "StepStarted",
            new
            {
                WorkflowStepId = @event.WorkflowStepId,
                @event.WorkflowId,
                @event.OccurredAt,
                @event.CorrelationId
            },
            cancellationToken);
}

public sealed class StepCompletedSignalrHandler(IHubContext<WorkflowHub> hubContext) : IEventHandler<WorkflowStepCompletedEvent>
{
    public Task HandleAsync(WorkflowStepCompletedEvent @event, CancellationToken cancellationToken) =>
        hubContext.Clients.Group(WorkflowHub.WorkflowGroup(@event.WorkflowId)).SendAsync(
            "StepCompleted",
            new
            {
                WorkflowStepId = @event.WorkflowStepId,
                @event.WorkflowId,
                @event.OccurredAt,
                @event.CorrelationId
            },
            cancellationToken);
}

public sealed class StepFailedSignalrHandler(IHubContext<WorkflowHub> hubContext) : IEventHandler<WorkflowStepFailedEvent>
{
    public Task HandleAsync(WorkflowStepFailedEvent @event, CancellationToken cancellationToken) =>
        hubContext.Clients.Group(WorkflowHub.WorkflowGroup(@event.WorkflowId)).SendAsync(
            "StepFailed",
            new
            {
                WorkflowStepId = @event.WorkflowStepId,
                @event.WorkflowId,
                @event.Reason,
                @event.OccurredAt,
                @event.CorrelationId
            },
            cancellationToken);
}

public sealed class LogReceivedSignalrHandler(IHubContext<WorkflowHub> hubContext) : IEventHandler<LogReceivedEvent>
{
    public Task HandleAsync(LogReceivedEvent @event, CancellationToken cancellationToken) =>
        hubContext.Clients.Group(WorkflowHub.WorkflowGroup(@event.WorkflowId)).SendAsync(
            "LogReceived",
            new
            {
                @event.WorkflowId,
                @event.Severity,
                @event.Message,
                @event.OccurredAt,
                @event.CorrelationId
            },
            cancellationToken);
}

public sealed class ToolExecutionStartedSignalrHandler(IHubContext<WorkflowHub> hubContext) : IEventHandler<ToolExecutionStartedEvent>
{
    public Task HandleAsync(ToolExecutionStartedEvent @event, CancellationToken cancellationToken) =>
        hubContext.Clients.Group(WorkflowHub.WorkflowGroup(@event.WorkflowId)).SendAsync(
            "ToolExecutionStarted",
            new
            {
                @event.WorkflowId,
                @event.ToolName,
                @event.OccurredAt,
                @event.CorrelationId
            },
            cancellationToken);
}

public sealed class ToolExecutionCompletedSignalrHandler(IHubContext<WorkflowHub> hubContext) : IEventHandler<ToolExecutionCompletedEvent>
{
    public Task HandleAsync(ToolExecutionCompletedEvent @event, CancellationToken cancellationToken) =>
        hubContext.Clients.Group(WorkflowHub.WorkflowGroup(@event.WorkflowId)).SendAsync(
            "ToolExecutionCompleted",
            new
            {
                @event.WorkflowId,
                @event.ToolName,
                @event.Succeeded,
                @event.OccurredAt,
                @event.CorrelationId
            },
            cancellationToken);
}

public sealed class ApprovalRequestedSignalrHandler(IHubContext<WorkflowHub> hubContext) : IEventHandler<ApprovalRequestedEvent>
{
    public Task HandleAsync(ApprovalRequestedEvent @event, CancellationToken cancellationToken) =>
        hubContext.Clients.Group(WorkflowHub.WorkflowGroup(@event.WorkflowId)).SendAsync(
            "ApprovalRequested",
            new
            {
                ApprovalId = @event.ApprovalRequestId,
                @event.WorkflowId,
                @event.OccurredAt,
                @event.CorrelationId
            },
            cancellationToken);
}

public sealed class WorkflowCompletedSignalrHandler(IHubContext<WorkflowHub> hubContext) : IEventHandler<WorkflowCompletedEvent>
{
    public Task HandleAsync(WorkflowCompletedEvent @event, CancellationToken cancellationToken) =>
        hubContext.Clients.Group(WorkflowHub.WorkflowGroup(@event.WorkflowId)).SendAsync(
            "WorkflowCompleted",
            new
            {
                @event.WorkflowId,
                @event.OccurredAt,
                @event.CorrelationId
            },
            cancellationToken);
}

public sealed class WorkflowFailedSignalrHandler(IHubContext<WorkflowHub> hubContext) : IEventHandler<WorkflowFailedEvent>
{
    public Task HandleAsync(WorkflowFailedEvent @event, CancellationToken cancellationToken) =>
        hubContext.Clients.Group(WorkflowHub.WorkflowGroup(@event.WorkflowId)).SendAsync(
            "WorkflowFailed",
            new
            {
                @event.WorkflowId,
                @event.Reason,
                @event.OccurredAt,
                @event.CorrelationId
            },
            cancellationToken);
}

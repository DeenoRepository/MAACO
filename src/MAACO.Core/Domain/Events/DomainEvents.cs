namespace MAACO.Core.Domain.Events;

public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
    string? CorrelationId { get; }
}

public sealed record TaskCreatedEvent(
    Guid TaskId,
    Guid ProjectId,
    DateTimeOffset OccurredAt,
    string? CorrelationId = null) : IDomainEvent;

public sealed record WorkflowStartedEvent(
    Guid WorkflowId,
    Guid TaskId,
    DateTimeOffset OccurredAt,
    string? CorrelationId = null) : IDomainEvent;

public sealed record WorkflowStepStartedEvent(
    Guid WorkflowStepId,
    Guid WorkflowId,
    DateTimeOffset OccurredAt,
    string? CorrelationId = null) : IDomainEvent;

public sealed record WorkflowStepCompletedEvent(
    Guid WorkflowStepId,
    Guid WorkflowId,
    DateTimeOffset OccurredAt,
    string? CorrelationId = null) : IDomainEvent;

public sealed record WorkflowStepFailedEvent(
    Guid WorkflowStepId,
    Guid WorkflowId,
    string Reason,
    DateTimeOffset OccurredAt,
    string? CorrelationId = null) : IDomainEvent;

public sealed record ApprovalRequestedEvent(
    Guid ApprovalRequestId,
    Guid WorkflowId,
    DateTimeOffset OccurredAt,
    string? CorrelationId = null) : IDomainEvent;

public sealed record WorkflowCompletedEvent(
    Guid WorkflowId,
    DateTimeOffset OccurredAt,
    string? CorrelationId = null) : IDomainEvent;

public sealed record WorkflowFailedEvent(
    Guid WorkflowId,
    string Reason,
    DateTimeOffset OccurredAt,
    string? CorrelationId = null) : IDomainEvent;

public sealed record LogReceivedEvent(
    Guid WorkflowId,
    string Severity,
    string Message,
    DateTimeOffset OccurredAt,
    string? CorrelationId = null) : IDomainEvent;

public sealed record ToolExecutionStartedEvent(
    Guid WorkflowId,
    string ToolName,
    DateTimeOffset OccurredAt,
    string? CorrelationId = null) : IDomainEvent;

public sealed record ToolExecutionCompletedEvent(
    Guid WorkflowId,
    string ToolName,
    bool Succeeded,
    DateTimeOffset OccurredAt,
    string? CorrelationId = null) : IDomainEvent;

namespace MAACO.Core.Domain.Events;

public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}

public sealed record TaskCreatedEvent(Guid TaskId, Guid ProjectId, DateTimeOffset OccurredAt) : IDomainEvent;

public sealed record WorkflowStartedEvent(Guid WorkflowId, Guid TaskId, DateTimeOffset OccurredAt) : IDomainEvent;

public sealed record WorkflowStepStartedEvent(Guid WorkflowStepId, Guid WorkflowId, DateTimeOffset OccurredAt) : IDomainEvent;

public sealed record WorkflowStepCompletedEvent(Guid WorkflowStepId, Guid WorkflowId, DateTimeOffset OccurredAt) : IDomainEvent;

public sealed record WorkflowStepFailedEvent(
    Guid WorkflowStepId,
    Guid WorkflowId,
    string Reason,
    DateTimeOffset OccurredAt) : IDomainEvent;

public sealed record ApprovalRequestedEvent(
    Guid ApprovalRequestId,
    Guid WorkflowId,
    DateTimeOffset OccurredAt) : IDomainEvent;

public sealed record WorkflowCompletedEvent(Guid WorkflowId, DateTimeOffset OccurredAt) : IDomainEvent;

public sealed record WorkflowFailedEvent(Guid WorkflowId, string Reason, DateTimeOffset OccurredAt) : IDomainEvent;

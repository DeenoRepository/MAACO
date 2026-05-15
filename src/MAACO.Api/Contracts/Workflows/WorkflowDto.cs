namespace MAACO.Api.Contracts.Workflows;

public sealed record WorkflowDto(
    Guid Id,
    Guid TaskId,
    string Status,
    string? FailureReason,
    int RetryCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    long Version);

namespace MAACO.Api.Contracts.Workflows;

public sealed record WorkflowDto(
    Guid Id,
    Guid TaskId,
    string Status,
    int RetryCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    long Version);

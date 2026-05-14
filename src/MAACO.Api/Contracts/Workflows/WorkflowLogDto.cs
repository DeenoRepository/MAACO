namespace MAACO.Api.Contracts.Workflows;

public sealed record WorkflowLogDto(
    Guid Id,
    Guid? WorkflowId,
    Guid? TaskId,
    string Severity,
    string Message,
    string? CorrelationId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    long Version);

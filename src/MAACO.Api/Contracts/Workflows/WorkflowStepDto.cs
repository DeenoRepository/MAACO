namespace MAACO.Api.Contracts.Workflows;

public sealed record WorkflowStepDto(
    Guid Id,
    Guid WorkflowId,
    string Name,
    string Status,
    int Order,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    long Version);

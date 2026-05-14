namespace MAACO.Api.Contracts.Workflows;

public sealed record WorkflowArtifactDto(
    Guid Id,
    Guid TaskId,
    string Type,
    string Path,
    string? Hash,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    long Version);

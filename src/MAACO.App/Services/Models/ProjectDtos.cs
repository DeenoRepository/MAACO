namespace MAACO.App.Services.Models;

public sealed record ProjectDto(
    Guid Id,
    string Name,
    string RepositoryPath,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    long Version);

public sealed record ProjectScanResponse(
    Guid ProjectId,
    string Status,
    string Message,
    int ScannedFiles,
    int SkippedBySize,
    int SkippedByLimit,
    string PrimaryStack,
    bool HasDotNet,
    bool HasNodeJs,
    bool HasPython,
    IReadOnlyList<string> SolutionFiles,
    IReadOnlyList<string> ProjectFiles,
    IReadOnlyList<string> PackageManifests,
    string BuildCommand,
    string TestCommand,
    bool IsCommandOverrideApplied);

public sealed record ProjectCreateResult(
    ProjectDto? Project,
    string? ErrorMessage);

public sealed record TaskDto(
    Guid Id,
    Guid ProjectId,
    string Title,
    string? Description,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    long Version);

public sealed record WorkflowStartResponse(
    Guid WorkflowId,
    string Status,
    string Message);

public sealed record WorkflowDto(
    Guid Id,
    Guid TaskId,
    string Status,
    string? FailureReason,
    int RetryCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    long Version);

public sealed record WorkflowStepDto(
    Guid Id,
    Guid WorkflowId,
    string Name,
    string Status,
    int Order,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    long Version);

public sealed record TaskDiffResponse(
    Guid TaskId,
    string Diff,
    string Status);

public sealed record TaskActionResponse(
    Guid TaskId,
    string Status,
    string Message);

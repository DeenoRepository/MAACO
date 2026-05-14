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

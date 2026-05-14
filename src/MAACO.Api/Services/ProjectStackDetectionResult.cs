namespace MAACO.Api.Services;

public sealed record ProjectStackDetectionResult(
    string PrimaryStack,
    bool HasDotNet,
    bool HasNodeJs,
    bool HasPython,
    IReadOnlyList<string> SolutionFiles,
    IReadOnlyList<string> ProjectFiles,
    IReadOnlyList<string> PackageManifests);

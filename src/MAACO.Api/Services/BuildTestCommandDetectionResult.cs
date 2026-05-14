namespace MAACO.Api.Services;

public sealed record BuildTestCommandDetectionResult(
    string BuildCommand,
    string TestCommand,
    bool IsOverrideApplied);

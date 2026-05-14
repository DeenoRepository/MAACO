namespace MAACO.Api.Services;

public sealed record ProjectScanResult(
    IReadOnlyList<string> Files,
    int ScannedFiles,
    int SkippedBySize,
    int SkippedByLimit);

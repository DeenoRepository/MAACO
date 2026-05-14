namespace MAACO.Core.Abstractions.Sandbox;

public sealed record SandboxOptions(
    TimeSpan Timeout,
    string? WorkingDirectory = null,
    IReadOnlyDictionary<string, string>? EnvironmentVariables = null,
    bool CaptureStdOut = true,
    bool CaptureStdErr = true);

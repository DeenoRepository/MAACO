namespace MAACO.Core.Abstractions.Sandbox;

public sealed record SandboxResult(
    bool Succeeded,
    int ExitCode,
    string StdOut,
    string StdErr,
    TimeSpan Duration,
    bool TimedOut = false,
    bool Cancelled = false,
    string? Error = null);

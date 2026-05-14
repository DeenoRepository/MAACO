namespace MAACO.Core.Abstractions.Tools;

public sealed record ToolResult(
    bool Succeeded,
    string Output,
    string? Error,
    TimeSpan Duration,
    bool TimedOut = false,
    string? CorrelationId = null);

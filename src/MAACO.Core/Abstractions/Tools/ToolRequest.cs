namespace MAACO.Core.Abstractions.Tools;

public sealed record ToolRequest(
    string ToolName,
    string Input,
    string WorkspacePath,
    IReadOnlyCollection<ToolPermission> Permissions,
    TimeSpan? Timeout = null,
    string? CorrelationId = null);

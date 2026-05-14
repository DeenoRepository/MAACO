namespace MAACO.Agents.Abstractions;

public sealed record AgentResult(
    bool Succeeded,
    string Output,
    string? Error,
    IReadOnlyDictionary<string, string>? Metadata = null,
    TimeSpan? Duration = null);

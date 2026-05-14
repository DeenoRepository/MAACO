namespace MAACO.Core.Abstractions.Memory;

public sealed record AgentMemoryContext(
    Guid ProjectId,
    IReadOnlyList<string> ProjectSummaries,
    IReadOnlyList<string> TaskSummaries,
    IReadOnlyList<string> Decisions,
    IReadOnlyList<string> BuildFailures,
    IReadOnlyList<string> AgentNotes,
    IReadOnlyList<string> FileSummaries);

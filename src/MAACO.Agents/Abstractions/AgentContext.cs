namespace MAACO.Agents.Abstractions;

public sealed record AgentContext(
    Guid ProjectId,
    Guid TaskId,
    Guid WorkflowId,
    string Instruction,
    IReadOnlyDictionary<string, string>? Inputs = null,
    string? CorrelationId = null);

namespace MAACO.Core.Abstractions.Workflows;

public sealed record WorkflowExecutionContext(
    Guid ProjectId,
    Guid TaskId,
    Guid WorkflowId,
    string Trigger,
    string? CorrelationId = null,
    IReadOnlyDictionary<string, string>? Inputs = null);

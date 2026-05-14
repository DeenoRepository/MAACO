namespace MAACO.Core.Abstractions.Llm;

public sealed record LlmRequest(
    IReadOnlyList<LlmMessage> Messages,
    string? Model = null,
    decimal? Temperature = null,
    int? MaxTokens = null,
    string? CorrelationId = null,
    Guid? WorkflowId = null);

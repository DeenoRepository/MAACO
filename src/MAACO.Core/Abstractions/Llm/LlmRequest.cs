namespace MAACO.Core.Abstractions.Llm;

public enum LlmTaskType
{
    Unknown = 0,
    Planning = 1,
    Coding = 2,
    Debugging = 3,
    Summary = 4
}

public sealed record LlmRequest(
    IReadOnlyList<LlmMessage> Messages,
    string? Model = null,
    LlmTaskType TaskType = LlmTaskType.Unknown,
    decimal? Temperature = null,
    int? MaxTokens = null,
    string? CorrelationId = null,
    Guid? WorkflowId = null);

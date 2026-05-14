namespace MAACO.Core.Abstractions.Llm;

public enum LlmMessageRole
{
    System = 0,
    User = 1,
    Assistant = 2,
    Tool = 3
}

public sealed record LlmMessage(
    LlmMessageRole Role,
    string Content,
    string? Name = null);

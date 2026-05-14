using MAACO.Core.Domain.ValueObjects;

namespace MAACO.Core.Abstractions.Llm;

public sealed record LlmResponse(
    bool Succeeded,
    string Content,
    LlmUsage Usage,
    string Provider,
    string Model,
    TimeSpan Duration,
    string? Error = null);

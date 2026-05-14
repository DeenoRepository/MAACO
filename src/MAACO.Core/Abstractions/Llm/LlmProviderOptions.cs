namespace MAACO.Core.Abstractions.Llm;

public sealed record LlmProviderOptions(
    string Provider,
    string DefaultModel,
    string? BaseUrl = null,
    string? ApiKey = null,
    TimeSpan? Timeout = null,
    int MaxRetryCount = 0);

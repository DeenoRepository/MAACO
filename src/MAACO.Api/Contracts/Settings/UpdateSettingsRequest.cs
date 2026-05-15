namespace MAACO.Api.Contracts.Settings;

public sealed record UpdateSettingsRequest(
    string LlmProvider,
    string LlmModel,
    bool RequireApproval,
    int MaxParallelAgents,
    string? ProviderBaseUrl = null,
    string? ApiKey = null,
    string? BuildCommandOverride = null,
    string? TestCommandOverride = null);

namespace MAACO.Api.Contracts.Settings;

public sealed record SettingsDto(
    string LlmProvider,
    string LlmModel,
    bool RequireApproval,
    int MaxParallelAgents,
    string? ProviderBaseUrl = null,
    bool HasApiKey = false,
    string? BuildCommandOverride = null,
    string? TestCommandOverride = null);

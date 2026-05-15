namespace MAACO.App.Services.Models;

public sealed record SettingsDto(
    string LlmProvider,
    string LlmModel,
    bool RequireApproval,
    int MaxParallelAgents,
    string? ProviderBaseUrl,
    bool HasApiKey,
    string? BuildCommandOverride,
    string? TestCommandOverride);

public sealed record UpdateSettingsRequest(
    string LlmProvider,
    string LlmModel,
    bool RequireApproval,
    int MaxParallelAgents,
    string? ProviderBaseUrl,
    string? ApiKey,
    string? BuildCommandOverride,
    string? TestCommandOverride);

public sealed record ProviderConnectionTestRequest(
    string LlmProvider,
    string LlmModel,
    string? ProviderBaseUrl,
    string? ApiKey);

public sealed record ProviderConnectionTestResultDto(
    bool Success,
    string Provider,
    bool IsSimulation,
    string Message);

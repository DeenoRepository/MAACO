namespace MAACO.Api.Contracts.Settings;

public sealed record SettingsDto(
    string LlmProvider,
    string LlmModel,
    bool RequireApproval,
    int MaxParallelAgents);

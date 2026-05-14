namespace MAACO.Api.Contracts.Settings;

public sealed record UpdateSettingsRequest(
    string LlmProvider,
    string LlmModel,
    bool RequireApproval,
    int MaxParallelAgents);

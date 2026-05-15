namespace MAACO.Api.Contracts.Settings;

public sealed record TestProviderConnectionRequest(
    string LlmProvider,
    string LlmModel,
    string? ProviderBaseUrl = null,
    string? ApiKey = null);

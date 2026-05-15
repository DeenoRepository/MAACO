namespace MAACO.Api.Contracts.Settings;

public sealed record ProviderConnectionTestResultDto(
    bool Success,
    string Provider,
    bool IsSimulation,
    string Message);

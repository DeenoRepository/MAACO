using MAACO.App.Services.Models;

namespace MAACO.App.Services;

public interface ISettingsClient
{
    Task<SettingsDto?> GetSettingsAsync(CancellationToken cancellationToken);
    Task<SettingsDto?> UpdateSettingsAsync(UpdateSettingsRequest request, CancellationToken cancellationToken);
    Task<ProviderConnectionTestResultDto?> TestConnectionAsync(ProviderConnectionTestRequest request, CancellationToken cancellationToken);
}

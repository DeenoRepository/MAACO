using MAACO.Api.Contracts.Settings;

namespace MAACO.Api.Services;

public interface ISettingsService
{
    Task<SettingsDto> GetAsync(CancellationToken cancellationToken);
    Task<SettingsDto> UpdateAsync(UpdateSettingsRequest request, CancellationToken cancellationToken);
}

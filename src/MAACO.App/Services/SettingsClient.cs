using MAACO.App.Services.Models;
using System.Net.Http.Json;

namespace MAACO.App.Services;

public sealed class SettingsClient(HttpClient httpClient) : ISettingsClient
{
    public Task<SettingsDto?> GetSettingsAsync(CancellationToken cancellationToken) =>
        httpClient.GetFromJsonAsync<SettingsDto>("api/settings", cancellationToken);

    public async Task<SettingsDto?> UpdateSettingsAsync(UpdateSettingsRequest request, CancellationToken cancellationToken)
    {
        var response = await httpClient.PutAsJsonAsync("api/settings", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<SettingsDto>(cancellationToken: cancellationToken);
    }

    public async Task<ProviderConnectionTestResultDto?> TestConnectionAsync(ProviderConnectionTestRequest request, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync("api/settings/test-connection", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<ProviderConnectionTestResultDto>(cancellationToken: cancellationToken);
    }
}

using System.Net.Http;

namespace MAACO.App.Services;

public sealed class ApiClient(HttpClient httpClient) : IApiClient
{
    public async Task<bool> CheckHealthAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.GetAsync("api/workflows", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}

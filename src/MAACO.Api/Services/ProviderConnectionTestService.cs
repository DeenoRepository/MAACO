using System.Net.Http.Headers;
using System.Text.Json;
using MAACO.Api.Contracts.Settings;

namespace MAACO.Api.Services;

public sealed class ProviderConnectionTestService(IHttpClientFactory httpClientFactory) : IProviderConnectionTestService
{
    public async Task<ProviderConnectionTestResultDto> TestAsync(
        TestProviderConnectionRequest request,
        CancellationToken cancellationToken)
    {
        var provider = request.LlmProvider?.Trim() ?? string.Empty;
        if (provider.Equals("Fake", StringComparison.OrdinalIgnoreCase))
        {
            return new ProviderConnectionTestResultDto(
                Success: true,
                Provider: "Fake",
                IsSimulation: true,
                Message: "Fake provider selected. This is a simulation mode.");
        }

        if (provider.Equals("OpenAI-Compatible", StringComparison.OrdinalIgnoreCase))
        {
            return await TestOpenAiCompatibleAsync(request, cancellationToken);
        }

        if (provider.Equals("Ollama", StringComparison.OrdinalIgnoreCase))
        {
            return await TestOllamaAsync(request, cancellationToken);
        }

        return new ProviderConnectionTestResultDto(
            Success: false,
            Provider: provider,
            IsSimulation: false,
            Message: $"Unsupported provider: {provider}");
    }

    private async Task<ProviderConnectionTestResultDto> TestOpenAiCompatibleAsync(
        TestProviderConnectionRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ProviderBaseUrl))
        {
            return new ProviderConnectionTestResultDto(false, "OpenAI-Compatible", false, "Provider base URL is required.");
        }

        if (string.IsNullOrWhiteSpace(request.ApiKey))
        {
            return new ProviderConnectionTestResultDto(false, "OpenAI-Compatible", false, "API key is required.");
        }

        var client = httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(20);
        client.BaseAddress = BuildBaseUri(request.ProviderBaseUrl!);

        using var message = new HttpRequestMessage(HttpMethod.Get, "models");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", request.ApiKey.Trim());

        try
        {
            using var response = await client.SendAsync(message, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return new ProviderConnectionTestResultDto(
                    true,
                    "OpenAI-Compatible",
                    false,
                    $"Connected successfully to {client.BaseAddress}.");
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            return new ProviderConnectionTestResultDto(
                false,
                "OpenAI-Compatible",
                false,
                $"Connection failed: HTTP {(int)response.StatusCode}. {Trim(body)}");
        }
        catch (Exception ex)
        {
            return new ProviderConnectionTestResultDto(false, "OpenAI-Compatible", false, $"Connection error: {ex.Message}");
        }
    }

    private async Task<ProviderConnectionTestResultDto> TestOllamaAsync(
        TestProviderConnectionRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ProviderBaseUrl))
        {
            return new ProviderConnectionTestResultDto(false, "Ollama", false, "Provider base URL is required.");
        }

        var client = httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(20);
        client.BaseAddress = BuildBaseUri(request.ProviderBaseUrl!);

        try
        {
            using var response = await client.GetAsync("api/tags", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                return new ProviderConnectionTestResultDto(
                    false,
                    "Ollama",
                    false,
                    $"Connection failed: HTTP {(int)response.StatusCode}. {Trim(body)}");
            }

            var payload = await response.Content.ReadAsStringAsync(cancellationToken);
            using var json = JsonDocument.Parse(payload);
            var count = json.RootElement.TryGetProperty("models", out var models)
                && models.ValueKind == JsonValueKind.Array
                ? models.GetArrayLength()
                : 0;

            return new ProviderConnectionTestResultDto(
                true,
                "Ollama",
                false,
                $"Connected successfully. Models detected: {count}.");
        }
        catch (Exception ex)
        {
            return new ProviderConnectionTestResultDto(false, "Ollama", false, $"Connection error: {ex.Message}");
        }
    }

    private static Uri BuildBaseUri(string baseUrl)
    {
        var normalized = baseUrl.Trim().TrimEnd('/') + "/";
        return new Uri(normalized, UriKind.Absolute);
    }

    private static string Trim(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.Length <= 300 ? value : value[..300];
    }
}

using MAACO.Core.Abstractions.Llm;
using MAACO.Core.Domain.ValueObjects;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MAACO.Infrastructure.Llm;

public sealed class OpenAiCompatibleLlmProvider(HttpClient httpClient, LlmProviderOptions options) : ILlmProvider
{
    public string Name => "OpenAI-Compatible";

    public async Task<bool> HealthCheckAsync(CancellationToken cancellationToken)
    {
        using var linkedCts = CreateTimeoutCts(cancellationToken);
        using var request = new HttpRequestMessage(HttpMethod.Get, "models");
        ApplyAuth(request);
        using var response = await LlmHttpRetry.SendWithRetryAsync(
            ct => httpClient.SendAsync(request.Clone(), ct),
            options.MaxRetryCount,
            linkedCts.Token);
        return response.IsSuccessStatusCode;
    }

    public async Task<LlmResponse> GenerateAsync(LlmRequest request, CancellationToken cancellationToken)
    {
        using var linkedCts = CreateTimeoutCts(cancellationToken);
        var startedAt = DateTimeOffset.UtcNow;
        using var message = BuildRequest(request);
        using var response = await LlmHttpRetry.SendWithRetryAsync(
            ct => httpClient.SendAsync(message.Clone(), ct),
            options.MaxRetryCount,
            linkedCts.Token);

        var payload = await response.Content.ReadAsStringAsync(linkedCts.Token);
        if (!response.IsSuccessStatusCode)
        {
            return new LlmResponse(
                Succeeded: false,
                Content: string.Empty,
                Usage: new LlmUsage(0, 0, 0, request.Model),
                Provider: Name,
                Model: request.Model ?? options.DefaultModel,
                Duration: DateTimeOffset.UtcNow - startedAt,
                Error: $"OpenAI-compatible request failed: {(int)response.StatusCode} {payload}");
        }

        using var document = JsonDocument.Parse(payload);
        var root = document.RootElement;
        var content = root.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? string.Empty;
        var usageElement = root.TryGetProperty("usage", out var usage) ? usage : default;
        var promptTokens = usageElement.ValueKind == JsonValueKind.Object && usageElement.TryGetProperty("prompt_tokens", out var p) ? p.GetInt32() : 0;
        var completionTokens = usageElement.ValueKind == JsonValueKind.Object && usageElement.TryGetProperty("completion_tokens", out var c) ? c.GetInt32() : 0;
        var totalTokens = usageElement.ValueKind == JsonValueKind.Object && usageElement.TryGetProperty("total_tokens", out var t) ? t.GetInt32() : (promptTokens + completionTokens);

        return new LlmResponse(
            Succeeded: true,
            Content: content,
            Usage: new LlmUsage(promptTokens, completionTokens, totalTokens, request.Model ?? options.DefaultModel),
            Provider: Name,
            Model: request.Model ?? options.DefaultModel,
            Duration: DateTimeOffset.UtcNow - startedAt);
    }

    private CancellationTokenSource CreateTimeoutCts(CancellationToken cancellationToken)
    {
        var timeout = options.Timeout ?? TimeSpan.FromSeconds(30);
        var timeoutCts = new CancellationTokenSource(timeout);
        return CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
    }

    private HttpRequestMessage BuildRequest(LlmRequest request)
    {
        var body = new
        {
            model = request.Model ?? options.DefaultModel,
            messages = request.Messages.Select(m => new { role = MapRole(m.Role), content = m.Content, name = m.Name }),
            temperature = request.Temperature,
            max_tokens = request.MaxTokens
        };

        var json = JsonSerializer.Serialize(body);
        var message = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        ApplyAuth(message);
        return message;
    }

    private void ApplyAuth(HttpRequestMessage request)
    {
        if (!string.IsNullOrWhiteSpace(options.ApiKey))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);
        }
    }

    private static string MapRole(LlmMessageRole role) => role switch
    {
        LlmMessageRole.System => "system",
        LlmMessageRole.Assistant => "assistant",
        LlmMessageRole.Tool => "tool",
        _ => "user"
    };
}

internal static class HttpRequestMessageCloneExtensions
{
    public static HttpRequestMessage Clone(this HttpRequestMessage source)
    {
        var clone = new HttpRequestMessage(source.Method, source.RequestUri);
        foreach (var header in source.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (source.Content is not null)
        {
            var contentBytes = source.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
            clone.Content = new ByteArrayContent(contentBytes);
            foreach (var header in source.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return clone;
    }
}

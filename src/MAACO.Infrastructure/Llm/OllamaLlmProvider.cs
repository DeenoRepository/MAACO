using MAACO.Core.Abstractions.Llm;
using MAACO.Core.Domain.ValueObjects;
using System.Text;
using System.Text.Json;

namespace MAACO.Infrastructure.Llm;

public sealed class OllamaLlmProvider(HttpClient httpClient, LlmProviderOptions options) : ILlmProvider
{
    public string Name => "Ollama";

    public async Task<bool> HealthCheckAsync(CancellationToken cancellationToken)
    {
        using var linkedCts = CreateTimeoutCts(cancellationToken);
        using var response = await LlmHttpRetry.SendWithRetryAsync(
            ct => httpClient.GetAsync("/api/tags", ct),
            options.MaxRetryCount,
            linkedCts.Token);
        return response.IsSuccessStatusCode;
    }

    public async Task<LlmResponse> GenerateAsync(LlmRequest request, CancellationToken cancellationToken)
    {
        using var linkedCts = CreateTimeoutCts(cancellationToken);
        var startedAt = DateTimeOffset.UtcNow;
        var body = new
        {
            model = request.Model ?? options.DefaultModel,
            messages = request.Messages.Select(m => new { role = MapRole(m.Role), content = m.Content }),
            stream = false,
            options = new
            {
                temperature = request.Temperature
            }
        };

        var json = JsonSerializer.Serialize(body);
        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/chat")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

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
                Error: $"Ollama request failed: {(int)response.StatusCode} {payload}");
        }

        using var document = JsonDocument.Parse(payload);
        var root = document.RootElement;
        var content = root.GetProperty("message").GetProperty("content").GetString() ?? string.Empty;
        var promptTokens = root.TryGetProperty("prompt_eval_count", out var p) ? p.GetInt32() : 0;
        var completionTokens = root.TryGetProperty("eval_count", out var c) ? c.GetInt32() : 0;
        var totalTokens = promptTokens + completionTokens;

        return new LlmResponse(
            Succeeded: true,
            Content: content,
            Usage: new LlmUsage(promptTokens, completionTokens, totalTokens, request.Model ?? options.DefaultModel),
            Provider: Name,
            Model: request.Model ?? options.DefaultModel,
            Duration: DateTimeOffset.UtcNow - startedAt);
    }

    private static string MapRole(LlmMessageRole role) => role switch
    {
        LlmMessageRole.System => "system",
        LlmMessageRole.Assistant => "assistant",
        _ => "user"
    };

    private CancellationTokenSource CreateTimeoutCts(CancellationToken cancellationToken)
    {
        var timeout = options.Timeout ?? TimeSpan.FromSeconds(30);
        var timeoutCts = new CancellationTokenSource(timeout);
        return CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
    }
}

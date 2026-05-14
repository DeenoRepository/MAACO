using MAACO.Core.Abstractions.Llm;
using MAACO.Core.Domain.ValueObjects;

namespace MAACO.Infrastructure.Llm;

public sealed class FakeLlmProvider : ILlmProvider
{
    public string Name => "Fake";

    public Task<bool> HealthCheckAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(true);
    }

    public Task<LlmResponse> GenerateAsync(LlmRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var userMessage = request.Messages.LastOrDefault(x => x.Role == LlmMessageRole.User)?.Content ?? string.Empty;
        var content = $"[FAKE:{request.Model ?? "fake-model"}] {userMessage}".Trim();
        var usage = new LlmUsage(
            PromptTokens: Math.Max(1, request.Messages.Sum(x => x.Content.Length) / 4),
            CompletionTokens: Math.Max(1, content.Length / 4),
            TotalTokens: Math.Max(2, (request.Messages.Sum(x => x.Content.Length) + content.Length) / 4),
            Model: request.Model ?? "fake-model");

        var response = new LlmResponse(
            Succeeded: true,
            Content: content,
            Usage: usage,
            Provider: Name,
            Model: request.Model ?? "fake-model",
            Duration: TimeSpan.FromMilliseconds(1));

        return Task.FromResult(response);
    }
}

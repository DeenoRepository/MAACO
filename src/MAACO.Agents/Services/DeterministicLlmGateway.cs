using MAACO.Core.Abstractions.Llm;
using MAACO.Core.Domain.ValueObjects;

namespace MAACO.Agents.Services;

internal sealed class DeterministicLlmGateway : ILlmGateway
{
    public Task<LlmResponse> GenerateAsync(LlmRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = request.Messages.LastOrDefault(x => x.Role == LlmMessageRole.User)?.Content ?? string.Empty;
        var content = $"deterministic-response: {user}".Trim();
        var usage = new LlmUsage(
            PromptTokens: Math.Max(1, user.Length / 4),
            CompletionTokens: Math.Max(1, content.Length / 5),
            TotalTokens: Math.Max(2, user.Length / 4 + content.Length / 5),
            Model: request.Model ?? "deterministic-fallback");

        return Task.FromResult(new LlmResponse(
            Succeeded: true,
            Content: content,
            Usage: usage,
            Provider: "DeterministicFallback",
            Model: request.Model ?? "deterministic-fallback",
            Duration: TimeSpan.FromMilliseconds(1)));
    }
}

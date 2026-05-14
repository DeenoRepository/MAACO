using MAACO.Core.Abstractions.Llm;
using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Domain.Entities;
using MAACO.Core.Domain.ValueObjects;
using System.Text.Json;

namespace MAACO.Infrastructure.Llm;

public sealed class LlmGateway(
    IEnumerable<ILlmProvider> providers,
    LlmProviderOptions providerOptions,
    ModelRoutingPolicy routingPolicy,
    ILlmCallLogRepository? llmCallLogRepository = null) : ILlmGateway
{
    private readonly IReadOnlyList<ILlmProvider> providerList = providers.ToList();

    public async Task<LlmResponse> GenerateAsync(LlmRequest request, CancellationToken cancellationToken)
    {
        var startedAt = DateTimeOffset.UtcNow;
        var provider = ResolveProvider();
        var model = ResolveModel(request);
        var effectiveRequest = request with { Model = model };
        var providerResponse = await provider.GenerateAsync(effectiveRequest, cancellationToken);
        var adjustedResponse = EnsureUsage(providerResponse, effectiveRequest);
        await PersistCallLogAsync(provider.Name, effectiveRequest, adjustedResponse, startedAt, cancellationToken);
        return adjustedResponse;
    }

    private ILlmProvider ResolveProvider()
    {
        var provider = providerList.FirstOrDefault(x =>
            string.Equals(x.Name, providerOptions.Provider, StringComparison.OrdinalIgnoreCase));

        return provider ?? providerList.First();
    }

    private string ResolveModel(LlmRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Model))
        {
            return request.Model;
        }

        return request.TaskType switch
        {
            LlmTaskType.Planning => routingPolicy.PlanningModel,
            LlmTaskType.Coding => routingPolicy.CodingModel,
            LlmTaskType.Debugging => routingPolicy.DebuggingModel,
            LlmTaskType.Summary => routingPolicy.SummaryModel,
            _ => string.IsNullOrWhiteSpace(providerOptions.DefaultModel)
                ? routingPolicy.FallbackModel
                : providerOptions.DefaultModel
        };
    }

    private static LlmResponse EnsureUsage(LlmResponse response, LlmRequest request)
    {
        var usage = response.Usage;
        if (usage.TotalTokens <= 0)
        {
            var prompt = Math.Max(1, request.Messages.Sum(x => x.Content.Length) / 4);
            var completion = Math.Max(1, response.Content.Length / 4);
            usage = new LlmUsage(prompt, completion, prompt + completion, response.Model);
        }

        return response with { Usage = usage };
    }

    private async Task PersistCallLogAsync(
        string providerName,
        LlmRequest request,
        LlmResponse response,
        DateTimeOffset startedAt,
        CancellationToken cancellationToken)
    {
        if (llmCallLogRepository is null)
        {
            return;
        }

        try
        {
            var redactedPrompt = LlmLogRedactor.Redact(string.Join("\n", request.Messages.Select(x => $"{x.Role}: {x.Content}")));
            var redactedResponse = LlmLogRedactor.Redact(response.Content);
            var estimatedCostUsd = LlmCostEstimator.EstimateUsd(response.Usage.TotalTokens);

            var callLog = new LlmCallLog
            {
                WorkflowId = request.WorkflowId,
                Provider = providerName,
                Model = response.Model,
                Usage = response.Usage,
                Duration = response.Duration == TimeSpan.Zero ? DateTimeOffset.UtcNow - startedAt : response.Duration,
                MetadataJson = JsonSerializer.Serialize(new
                {
                    correlationId = request.CorrelationId,
                    taskType = request.TaskType.ToString(),
                    succeeded = response.Succeeded,
                    prompt = Truncate(redactedPrompt, 4000),
                    response = Truncate(redactedResponse, 4000),
                    error = Truncate(LlmLogRedactor.Redact(response.Error), 2000),
                    estimatedCostUsd
                }),
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await llmCallLogRepository.AddAsync(callLog, cancellationToken);
            await llmCallLogRepository.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // LLM logging failures should not break execution in MVP.
        }
    }

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..max];
}

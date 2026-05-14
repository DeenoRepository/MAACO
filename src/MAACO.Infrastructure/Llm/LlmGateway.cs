using MAACO.Core.Abstractions.Llm;

namespace MAACO.Infrastructure.Llm;

public sealed class LlmGateway(
    IEnumerable<ILlmProvider> providers,
    LlmProviderOptions providerOptions,
    ModelRoutingPolicy routingPolicy) : ILlmGateway
{
    private readonly IReadOnlyList<ILlmProvider> providerList = providers.ToList();

    public async Task<LlmResponse> GenerateAsync(LlmRequest request, CancellationToken cancellationToken)
    {
        var provider = ResolveProvider();
        var model = ResolveModel(request);
        var effectiveRequest = request with { Model = model };
        return await provider.GenerateAsync(effectiveRequest, cancellationToken);
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
}

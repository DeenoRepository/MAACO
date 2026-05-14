namespace MAACO.Core.Abstractions.Llm;

public interface ILlmProvider
{
    string Name { get; }

    Task<bool> HealthCheckAsync(CancellationToken cancellationToken);

    Task<LlmResponse> GenerateAsync(
        LlmRequest request,
        CancellationToken cancellationToken);
}

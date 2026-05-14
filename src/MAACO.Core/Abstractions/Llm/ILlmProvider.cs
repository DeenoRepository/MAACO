namespace MAACO.Core.Abstractions.Llm;

public interface ILlmProvider
{
    string Name { get; }

    Task<LlmResponse> GenerateAsync(
        LlmRequest request,
        CancellationToken cancellationToken);
}

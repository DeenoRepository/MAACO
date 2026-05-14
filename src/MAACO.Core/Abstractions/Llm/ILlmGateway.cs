namespace MAACO.Core.Abstractions.Llm;

public interface ILlmGateway
{
    Task<LlmResponse> GenerateAsync(
        LlmRequest request,
        CancellationToken cancellationToken);
}

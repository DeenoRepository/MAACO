using MAACO.Core.Abstractions.Llm;
using MAACO.Core.Domain.ValueObjects;
using MAACO.Infrastructure.Llm;

namespace MAACO.Core.Tests;

public sealed class LlmGatewayRoutingTests
{
    [Fact]
    public async Task GenerateAsync_UsesRoutingPolicyModel_WhenModelNotProvided()
    {
        var gateway = new LlmGateway(
            providers: [new FakeLlmProvider()],
            providerOptions: new LlmProviderOptions("Fake", "fake-default"),
            routingPolicy: new ModelRoutingPolicy(
                PlanningModel: "model-planning",
                CodingModel: "model-coding",
                DebuggingModel: "model-debug",
                SummaryModel: "model-summary",
                FallbackModel: "model-fallback"));

        var response = await gateway.GenerateAsync(
            new LlmRequest(
                Messages: [new LlmMessage(LlmMessageRole.User, "Plan next step")],
                TaskType: LlmTaskType.Planning),
            CancellationToken.None);

        Assert.Equal("model-planning", response.Model);
        Assert.Contains("[FAKE:model-planning]", response.Content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GenerateAsync_UsesExplicitModel_WhenProvided()
    {
        var gateway = new LlmGateway(
            providers: [new FakeLlmProvider()],
            providerOptions: new LlmProviderOptions("Fake", "fake-default"),
            routingPolicy: new ModelRoutingPolicy(
                PlanningModel: "model-planning",
                CodingModel: "model-coding",
                DebuggingModel: "model-debug",
                SummaryModel: "model-summary",
                FallbackModel: "model-fallback"));

        var response = await gateway.GenerateAsync(
            new LlmRequest(
                Messages: [new LlmMessage(LlmMessageRole.User, "Generate patch")],
                Model: "manual-model",
                TaskType: LlmTaskType.Coding),
            CancellationToken.None);

        Assert.Equal("manual-model", response.Model);
        Assert.Contains("[FAKE:manual-model]", response.Content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GenerateAsync_UsesOpenAiProvider_WhenConfigured()
    {
        var openAi = new RecordingProvider("OpenAI-Compatible");
        var ollama = new RecordingProvider("Ollama");
        var gateway = new LlmGateway(
            providers: [openAi, ollama],
            providerOptions: new LlmProviderOptions("OpenAI-Compatible", "gpt-5"),
            routingPolicy: new ModelRoutingPolicy("p", "c", "d", "s", "f"));

        var response = await gateway.GenerateAsync(
            new LlmRequest([new LlmMessage(LlmMessageRole.User, "hello")]),
            CancellationToken.None);

        Assert.True(response.Succeeded);
        Assert.Equal(1, openAi.GenerateCalls);
        Assert.Equal(0, ollama.GenerateCalls);
        Assert.Equal("OpenAI-Compatible", response.Provider);
    }

    [Fact]
    public async Task GenerateAsync_UsesOllamaProvider_WhenConfigured()
    {
        var openAi = new RecordingProvider("OpenAI-Compatible");
        var ollama = new RecordingProvider("Ollama");
        var gateway = new LlmGateway(
            providers: [openAi, ollama],
            providerOptions: new LlmProviderOptions("Ollama", "llama3.1"),
            routingPolicy: new ModelRoutingPolicy("p", "c", "d", "s", "f"));

        var response = await gateway.GenerateAsync(
            new LlmRequest([new LlmMessage(LlmMessageRole.User, "hello")]),
            CancellationToken.None);

        Assert.True(response.Succeeded);
        Assert.Equal(0, openAi.GenerateCalls);
        Assert.Equal(1, ollama.GenerateCalls);
        Assert.Equal("Ollama", response.Provider);
    }

    private sealed class RecordingProvider(string name) : ILlmProvider
    {
        public string Name { get; } = name;
        public int GenerateCalls { get; private set; }

        public Task<bool> HealthCheckAsync(CancellationToken cancellationToken) => Task.FromResult(true);

        public Task<LlmResponse> GenerateAsync(LlmRequest request, CancellationToken cancellationToken)
        {
            GenerateCalls++;
            return Task.FromResult(new LlmResponse(
                Succeeded: true,
                Content: $"ok-{Name}",
                Usage: new LlmUsage(1, 1, 2, request.Model),
                Provider: Name,
                Model: request.Model ?? "model",
                Duration: TimeSpan.FromMilliseconds(1)));
        }
    }
}

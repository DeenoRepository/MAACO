using MAACO.Core.Abstractions.Llm;
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
}

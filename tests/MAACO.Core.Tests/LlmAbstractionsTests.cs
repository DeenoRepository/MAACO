using MAACO.Core.Abstractions.Llm;
using MAACO.Core.Domain.ValueObjects;

namespace MAACO.Core.Tests;

public sealed class LlmAbstractionsTests
{
    [Fact]
    public void LlmRequest_HoldsMessagesAndMetadata()
    {
        var request = new LlmRequest(
            Messages:
            [
                new LlmMessage(LlmMessageRole.System, "You are MAACO."),
                new LlmMessage(LlmMessageRole.User, "Generate patch")
            ],
            Model: "gpt-5",
            Temperature: 0.2m,
            MaxTokens: 2048,
            CorrelationId: "corr-llm-1",
            WorkflowId: Guid.NewGuid());

        Assert.Equal(2, request.Messages.Count);
        Assert.Equal(LlmMessageRole.System, request.Messages[0].Role);
        Assert.Equal("gpt-5", request.Model);
        Assert.Equal(0.2m, request.Temperature);
        Assert.Equal(2048, request.MaxTokens);
    }

    [Fact]
    public void LlmResponse_HoldsUsageAndStatus()
    {
        var response = new LlmResponse(
            Succeeded: true,
            Content: "Patch prepared.",
            Usage: new LlmUsage(120, 40, 160, Model: "gpt-5"),
            Provider: "OpenAI-Compatible",
            Model: "gpt-5",
            Duration: TimeSpan.FromMilliseconds(350));

        Assert.True(response.Succeeded);
        Assert.Equal("Patch prepared.", response.Content);
        Assert.Equal(160, response.Usage.TotalTokens);
        Assert.Equal("OpenAI-Compatible", response.Provider);
        Assert.Null(response.Error);
    }

    [Fact]
    public void ModelRoutingPolicy_HoldsModelAssignments()
    {
        var policy = new ModelRoutingPolicy(
            PlanningModel: "gpt-5",
            CodingModel: "gpt-5",
            DebuggingModel: "gpt-5",
            SummaryModel: "gpt-5-mini",
            FallbackModel: "gpt-5");

        Assert.Equal("gpt-5-mini", policy.SummaryModel);
        Assert.Equal("gpt-5", policy.FallbackModel);
    }
}

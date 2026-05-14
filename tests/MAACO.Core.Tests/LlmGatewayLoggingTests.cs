using MAACO.Core.Abstractions.Llm;
using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Domain.Entities;
using MAACO.Core.Domain.ValueObjects;
using MAACO.Infrastructure.Llm;
using System.Text.Json;

namespace MAACO.Core.Tests;

public sealed class LlmGatewayLoggingTests
{
    [Fact]
    public async Task GenerateAsync_PersistsRedactedLlmCallLog_WithEstimatedCost()
    {
        var repository = new InMemoryLlmCallLogRepository();
        var gateway = new LlmGateway(
            providers: [new FakeLlmProvider()],
            providerOptions: new LlmProviderOptions("Fake", "fake-default"),
            routingPolicy: new ModelRoutingPolicy("p", "c", "d", "s", "f"),
            llmCallLogRepository: repository);

        var workflowId = Guid.NewGuid();
        var response = await gateway.GenerateAsync(
            new LlmRequest(
                Messages: [new LlmMessage(LlmMessageRole.User, "token=abc123 generate patch")],
                TaskType: LlmTaskType.Coding,
                CorrelationId: "corr-llm-log-1",
                WorkflowId: workflowId),
            CancellationToken.None);

        Assert.True(response.Succeeded);
        Assert.Single(repository.Items);
        var log = repository.Items[0];
        Assert.Equal(workflowId, log.WorkflowId);
        Assert.True(log.Usage.TotalTokens > 0);
        Assert.False(string.IsNullOrWhiteSpace(log.MetadataJson));
        Assert.DoesNotContain("abc123", log.MetadataJson, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("***REDACTED***", log.MetadataJson, StringComparison.OrdinalIgnoreCase);

        using var doc = JsonDocument.Parse(log.MetadataJson!);
        Assert.True(doc.RootElement.TryGetProperty("estimatedCostUsd", out var costProp));
        Assert.True(costProp.GetDecimal() >= 0m);
    }

    [Fact]
    public async Task GenerateAsync_FillsUsage_WhenProviderReturnsZeroTokens()
    {
        var repository = new InMemoryLlmCallLogRepository();
        var gateway = new LlmGateway(
            providers: [new ZeroUsageProvider()],
            providerOptions: new LlmProviderOptions("Zero", "zero-model"),
            routingPolicy: new ModelRoutingPolicy("p", "c", "d", "s", "f"),
            llmCallLogRepository: repository);

        var response = await gateway.GenerateAsync(
            new LlmRequest([new LlmMessage(LlmMessageRole.User, "hello")]),
            CancellationToken.None);

        Assert.True(response.Usage.TotalTokens > 0);
        Assert.Single(repository.Items);
        Assert.True(repository.Items[0].Usage.TotalTokens > 0);
    }

    private sealed class InMemoryLlmCallLogRepository : ILlmCallLogRepository
    {
        public List<LlmCallLog> Items { get; } = [];

        public Task AddAsync(LlmCallLog llmCallLog, CancellationToken cancellationToken)
        {
            Items.Add(llmCallLog);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class ZeroUsageProvider : ILlmProvider
    {
        public string Name => "Zero";

        public Task<bool> HealthCheckAsync(CancellationToken cancellationToken) => Task.FromResult(true);

        public Task<LlmResponse> GenerateAsync(LlmRequest request, CancellationToken cancellationToken)
            => Task.FromResult(new LlmResponse(
                Succeeded: true,
                Content: "ok",
                Usage: new LlmUsage(0, 0, 0, "zero-model"),
                Provider: Name,
                Model: "zero-model",
                Duration: TimeSpan.FromMilliseconds(1)));
    }
}

using MAACO.Agents.Abstractions;
using MAACO.Agents.Services;

namespace MAACO.Agents.Tests;

public sealed class AgentRuntimeAbstractionsTests
{
    [Fact]
    public void AgentRegistry_ResolvesAgentByName_CaseInsensitive()
    {
        var agent = new FakeAgent("TaskPlannerAgent");
        var registry = new AgentRegistry([agent]);

        var resolved = registry.GetByName("taskplanneragent");

        Assert.NotNull(resolved);
        Assert.Equal("TaskPlannerAgent", resolved!.Name);
        Assert.Contains("TaskPlannerAgent", registry.ListAgentNames());
    }

    [Fact]
    public async Task AgentExecutionService_ExecutesAgentAndSetsDuration()
    {
        var agent = new FakeAgent("BackendDeveloperAgent");
        var registry = new AgentRegistry([agent]);
        var service = new AgentExecutionService(registry);
        var context = new AgentContext(
            ProjectId: Guid.NewGuid(),
            TaskId: Guid.NewGuid(),
            WorkflowId: Guid.NewGuid(),
            Instruction: "Generate implementation plan");

        var result = await service.ExecuteAsync("BackendDeveloperAgent", context, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal("ok", result.Output);
        Assert.NotNull(result.Duration);
        Assert.True(result.Duration > TimeSpan.Zero);
    }

    [Fact]
    public async Task AgentExecutionService_ReturnsFailure_WhenAgentMissing()
    {
        var registry = new AgentRegistry([]);
        var service = new AgentExecutionService(registry);
        var context = new AgentContext(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "hello");

        var result = await service.ExecuteAsync("MissingAgent", context, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("not registered", result.Error ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AgentExecutionService_RespectsCancellation()
    {
        var agent = new FakeAgent("DebuggingAgent");
        var registry = new AgentRegistry([agent]);
        var service = new AgentExecutionService(registry);
        var context = new AgentContext(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "debug");
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.ExecuteAsync("DebuggingAgent", context, cts.Token));
    }

    private sealed class FakeAgent(string name) : IAgent
    {
        public string Name { get; } = name;
        public string Role => "Fake";
        public IReadOnlyCollection<AgentCapability> Capabilities => [AgentCapability.Planning];

        public Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(new AgentResult(
                Succeeded: true,
                Output: "ok",
                Error: null,
                Metadata: new Dictionary<string, string> { ["agent"] = Name }));
        }
    }
}

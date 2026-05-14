using MAACO.Agents.Abstractions;
using MAACO.Agents.Agents;
using MAACO.Agents.Prompts;
using MAACO.Core.Abstractions.Tools;
using MAACO.Core.Abstractions.Llm;
using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Domain.Entities;
using MAACO.Agents.Services;
using MAACO.Infrastructure.Llm;
using Microsoft.Extensions.DependencyInjection;

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

    [Fact]
    public async Task AgentExecutionService_BlocksDirectFileMutationPayload()
    {
        var agent = new FakeAgent("BackendDeveloperAgent");
        var registry = new AgentRegistry([agent]);
        var service = new AgentExecutionService(registry);
        var context = new AgentContext(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "mutate file",
            new Dictionary<string, string>
            {
                ["operation"] = "write",
                ["path"] = "src/SomeFile.cs",
                ["content"] = "new content"
            });

        var result = await service.ExecuteAsync("BackendDeveloperAgent", context, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("forbidden", result.Error ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AgentExecutionService_AllowsToolDelegationPayload()
    {
        var tool = new FakeTool("DemoTool");
        var agent = new TaskPlannerAgent([tool], new DefaultAgentPromptCatalog());
        var registry = new AgentRegistry([agent]);
        var service = new AgentExecutionService(registry);
        var context = new AgentContext(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "plan",
            new Dictionary<string, string>
            {
                ["toolName"] = "DemoTool",
                ["toolInput"] = "ping",
                ["workspacePath"] = "D:/Projects/MAACO"
            });

        var result = await service.ExecuteAsync("TaskPlannerAgent", context, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(1, tool.CallCount);
    }

    [Fact]
    public async Task AddMaacoAgents_RegistersAllMilestoneAgents()
    {
        var services = new ServiceCollection();
        services.AddMaacoAgents();
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();

        var registry = scope.ServiceProvider.GetRequiredService<IAgentRegistry>();
        var execution = scope.ServiceProvider.GetRequiredService<IAgentExecutionService>();

        var expected = new[]
        {
            "OrchestratorAgent",
            "TaskPlannerAgent",
            "BackendDeveloperAgent",
            "TestWriterAgent",
            "DebuggingAgent",
            "GitManagerAgent",
            "DocumentationAgent"
        };

        foreach (var name in expected)
        {
            Assert.NotNull(registry.GetByName(name));
        }

        var result = await execution.ExecuteAsync(
            "TaskPlannerAgent",
            new AgentContext(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "plan"),
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Metadata);
        Assert.Equal("TaskPlannerAgent", result.Metadata!["agent"]);
        Assert.True(result.Metadata.ContainsKey("systemPrompt"));
        Assert.True(result.Metadata.ContainsKey("responseSchema"));
        Assert.True(result.Metadata.ContainsKey("decision"));
    }

    [Fact]
    public async Task AgentExecutionService_PersistsAgentOutputs_AsLogsAndArtifacts()
    {
        var logs = new InMemoryLogRepository();
        var artifacts = new InMemoryArtifactRepository();
        var agent = new FakeAgent("DocumentationAgent");
        var registry = new AgentRegistry([agent]);
        var service = new AgentExecutionService(registry, logs, artifacts);
        var context = new AgentContext(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "document");

        var result = await service.ExecuteAsync("DocumentationAgent", context, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Single(logs.Items);
        Assert.Single(artifacts.Items);
        Assert.Equal(context.TaskId, artifacts.Items[0].TaskId);
    }

    [Fact]
    public async Task AgentDemoWorkflowService_RunsWithFakeLlmProvider()
    {
        var llmGateway = new LlmGateway(
            providers: [new FakeLlmProvider()],
            providerOptions: new LlmProviderOptions("Fake", "fake-default"),
            routingPolicy: new ModelRoutingPolicy(
                PlanningModel: "fake-default",
                CodingModel: "fake-default",
                DebuggingModel: "fake-default",
                SummaryModel: "fake-default",
                FallbackModel: "fake-default"));

        var planner = new TaskPlannerAgent([], new DefaultAgentPromptCatalog());
        var registry = new AgentRegistry([planner]);
        var execution = new AgentExecutionService(registry);
        var demo = new AgentDemoWorkflowService(llmGateway, execution);
        var context = new AgentContext(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Plan demo workflow");

        var result = await demo.RunAsync(context, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Metadata);
        Assert.Equal("Fake", result.Metadata!["demoLlmProvider"]);
    }

    [Fact]
    public async Task AgentStub_ExecutesDelegatedTool_WhenRequested()
    {
        var tool = new FakeTool("DemoTool");
        var agent = new TaskPlannerAgent([tool], new DefaultAgentPromptCatalog());
        var context = new AgentContext(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "plan",
            new Dictionary<string, string>
            {
                ["toolName"] = "DemoTool",
                ["toolInput"] = "ping",
                ["workspacePath"] = "D:/Projects/MAACO"
            });

        var result = await agent.ExecuteAsync(context, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(1, tool.CallCount);
        Assert.Equal("executed", result.Metadata!["delegatedTool"]);
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

    private sealed class FakeTool(string name) : IAgentTool
    {
        public int CallCount { get; private set; }
        public string Name { get; } = name;
        public IReadOnlyCollection<ToolPermission> RequiredPermissions => [ToolPermission.ReadOnly];

        public Task<ToolResult> ExecuteAsync(ToolRequest request, CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(new ToolResult(
                Succeeded: true,
                Output: "ok",
                Error: null,
                Duration: TimeSpan.FromMilliseconds(1),
                CorrelationId: request.CorrelationId));
        }
    }

    private sealed class InMemoryLogRepository : ILogRepository
    {
        public List<LogEvent> Items { get; } = [];

        public Task AddAsync(LogEvent logEvent, CancellationToken cancellationToken)
        {
            Items.Add(logEvent);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<LogEvent>> ListByWorkflowIdAsync(Guid workflowId, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<LogEvent>>(Items.Where(x => x.WorkflowId == workflowId).ToList());

        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class InMemoryArtifactRepository : IArtifactRepository
    {
        public List<Artifact> Items { get; } = [];

        public Task AddAsync(Artifact artifact, CancellationToken cancellationToken)
        {
            Items.Add(artifact);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<Artifact>> ListByTaskIdAsync(Guid taskId, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<Artifact>>(Items.Where(x => x.TaskId == taskId).ToList());

        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}

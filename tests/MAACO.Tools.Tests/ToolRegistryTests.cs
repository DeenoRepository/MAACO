using MAACO.Core.Abstractions.Tools;
using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Domain.Entities;
using MAACO.Core.Domain.Enums;
using MAACO.Tools;
using Microsoft.Extensions.Logging.Abstractions;

namespace MAACO.Tools.Tests;

public sealed class ToolRegistryTests
{
    [Fact]
    public async Task ExecuteAsync_InvokesToolViaSingleInterface()
    {
        var registry = new ToolRegistry(
            [new SuccessTool()],
            NullLogger<ToolRegistry>.Instance);

        var request = new ToolRequest(
            "SuccessTool",
            "{}",
            "D:\\Projects\\MAACO",
            [ToolPermission.ReadOnly],
            CorrelationId: "corr-success");

        var result = await registry.ExecuteAsync(request, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal("ok", result.Output);
    }

    [Fact]
    public async Task ExecuteAsync_PersistsToolExecution_WhenWorkflowIdProvided()
    {
        var executionRepository = new InMemoryToolExecutionRepository();
        var registry = new ToolRegistry(
            [new SuccessTool()],
            NullLogger<ToolRegistry>.Instance,
            executionRepository);

        var workflowId = Guid.NewGuid();
        var request = new ToolRequest(
            "SuccessTool",
            $"{{\"workflowId\":\"{workflowId}\"}}",
            "D:\\Projects\\MAACO",
            [ToolPermission.ReadOnly],
            CorrelationId: "corr-persist");

        var result = await registry.ExecuteAsync(request, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Single(executionRepository.Items);
        var execution = executionRepository.Items[0];
        Assert.Equal(workflowId, execution.WorkflowId);
        Assert.Equal(ToolExecutionStatus.Completed, execution.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsControlledError_ForUnknownTool()
    {
        var registry = new ToolRegistry(
            [new SuccessTool()],
            NullLogger<ToolRegistry>.Instance);

        var request = new ToolRequest(
            "MissingTool",
            "{}",
            "D:\\Projects\\MAACO",
            [ToolPermission.ReadOnly],
            CorrelationId: "corr-unknown");

        var result = await registry.ExecuteAsync(request, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("is not registered", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsControlledError_ForInsufficientPermissions()
    {
        var registry = new ToolRegistry(
            [new WorkspaceWriteTool()],
            NullLogger<ToolRegistry>.Instance);

        var request = new ToolRequest(
            "WorkspaceWriteTool",
            "{}",
            "D:\\Projects\\MAACO",
            [ToolPermission.ReadOnly],
            CorrelationId: "corr-perm");

        var result = await registry.ExecuteAsync(request, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("Insufficient permissions", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsControlledError_ForToolException()
    {
        var registry = new ToolRegistry(
            [new ThrowingTool()],
            NullLogger<ToolRegistry>.Instance);

        var request = new ToolRequest(
            "ThrowingTool",
            "{}",
            "D:\\Projects\\MAACO",
            [ToolPermission.ReadOnly],
            CorrelationId: "corr-throw");

        var result = await registry.ExecuteAsync(request, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("Tool execution failed", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsTimedOutResult_WhenTimeoutExceeded()
    {
        var registry = new ToolRegistry(
            [new DelayedTool()],
            NullLogger<ToolRegistry>.Instance);

        var request = new ToolRequest(
            "DelayedTool",
            "{}",
            "D:\\Projects\\MAACO",
            [ToolPermission.ReadOnly],
            Timeout: TimeSpan.FromMilliseconds(30),
            CorrelationId: "corr-timeout");

        var result = await registry.ExecuteAsync(request, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.True(result.TimedOut);
        Assert.Contains("timed out", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsCancelledResult_WhenCallerCancels()
    {
        var registry = new ToolRegistry(
            [new DelayedTool()],
            NullLogger<ToolRegistry>.Instance);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var request = new ToolRequest(
            "DelayedTool",
            "{}",
            "D:\\Projects\\MAACO",
            [ToolPermission.ReadOnly],
            CorrelationId: "corr-cancel");

        var result = await registry.ExecuteAsync(request, cts.Token);

        Assert.False(result.Succeeded);
        Assert.False(result.TimedOut);
        Assert.Contains("cancelled", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class SuccessTool : IAgentTool
    {
        public string Name => "SuccessTool";
        public IReadOnlyCollection<ToolPermission> RequiredPermissions => [ToolPermission.ReadOnly];

        public Task<ToolResult> ExecuteAsync(ToolRequest request, CancellationToken cancellationToken) =>
            Task.FromResult(new ToolResult(true, "ok", null, TimeSpan.Zero, CorrelationId: request.CorrelationId));
    }

    private sealed class WorkspaceWriteTool : IAgentTool
    {
        public string Name => "WorkspaceWriteTool";
        public IReadOnlyCollection<ToolPermission> RequiredPermissions => [ToolPermission.WorkspaceWrite];

        public Task<ToolResult> ExecuteAsync(ToolRequest request, CancellationToken cancellationToken) =>
            Task.FromResult(new ToolResult(true, "ok", null, TimeSpan.Zero, CorrelationId: request.CorrelationId));
    }

    private sealed class ThrowingTool : IAgentTool
    {
        public string Name => "ThrowingTool";
        public IReadOnlyCollection<ToolPermission> RequiredPermissions => [ToolPermission.ReadOnly];

        public Task<ToolResult> ExecuteAsync(ToolRequest request, CancellationToken cancellationToken) =>
            throw new InvalidOperationException("boom");
    }

    private sealed class DelayedTool : IAgentTool
    {
        public string Name => "DelayedTool";
        public IReadOnlyCollection<ToolPermission> RequiredPermissions => [ToolPermission.ReadOnly];

        public async Task<ToolResult> ExecuteAsync(ToolRequest request, CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            return new ToolResult(true, "done", null, TimeSpan.Zero, CorrelationId: request.CorrelationId);
        }
    }

    private sealed class InMemoryToolExecutionRepository : IToolExecutionRepository
    {
        public List<ToolExecution> Items { get; } = [];

        public Task AddAsync(ToolExecution toolExecution, CancellationToken cancellationToken)
        {
            Items.Add(toolExecution);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}

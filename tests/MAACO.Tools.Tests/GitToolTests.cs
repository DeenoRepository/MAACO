using MAACO.Core.Abstractions.Tools;
using MAACO.Tools.Tools;

namespace MAACO.Tools.Tests;

public sealed class GitToolTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsFailure_WhenPathIsNotGitRepository()
    {
        var workspace = CreateWorkspace(isGitRepo: false);
        var tool = new GitTool();
        var request = new ToolRequest(
            tool.Name,
            "status",
            workspace,
            [ToolPermission.ReadOnly],
            CorrelationId: "corr-not-git");

        var result = await tool.ExecuteAsync(request, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("not a git repository", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_AllowsGitRepositoryAndValidatesOperation()
    {
        var workspace = CreateWorkspace(isGitRepo: true);
        var tool = new GitTool();
        var request = new ToolRequest(
            tool.Name,
            "unknown-op",
            workspace,
            [ToolPermission.ReadOnly],
            CorrelationId: "corr-git-op");

        var result = await tool.ExecuteAsync(request, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("Unsupported git operation", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_AcceptsStatusOperation_ForGitRepository()
    {
        var workspace = CreateWorkspace(isGitRepo: true);
        var tool = new GitTool();
        var request = new ToolRequest(
            tool.Name,
            "status",
            workspace,
            [ToolPermission.ReadOnly],
            CorrelationId: "corr-status");

        var result = await tool.ExecuteAsync(request, CancellationToken.None);

        Assert.DoesNotContain("Unsupported git operation", result.Error ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("not a git repository", result.Error ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_AcceptsCurrentBranchOperation_ForGitRepository()
    {
        var workspace = CreateWorkspace(isGitRepo: true);
        var tool = new GitTool();
        var request = new ToolRequest(
            tool.Name,
            "current-branch",
            workspace,
            [ToolPermission.ReadOnly],
            CorrelationId: "corr-branch");

        var result = await tool.ExecuteAsync(request, CancellationToken.None);

        Assert.DoesNotContain("Unsupported git operation", result.Error ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("not a git repository", result.Error ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    private static string CreateWorkspace(bool isGitRepo)
    {
        var path = Path.Combine(Path.GetTempPath(), "maaco-gittool-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        if (isGitRepo)
        {
            Directory.CreateDirectory(Path.Combine(path, ".git"));
        }

        return path;
    }
}

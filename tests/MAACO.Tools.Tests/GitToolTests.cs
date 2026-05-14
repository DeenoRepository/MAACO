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

    [Fact]
    public async Task ExecuteAsync_AcceptsDiffOperation_ForGitRepository()
    {
        var workspace = CreateWorkspace(isGitRepo: true);
        var tool = new GitTool();
        var request = new ToolRequest(
            tool.Name,
            "diff",
            workspace,
            [ToolPermission.ReadOnly],
            CorrelationId: "corr-diff");

        var result = await tool.ExecuteAsync(request, CancellationToken.None);

        Assert.DoesNotContain("Unsupported git operation", result.Error ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("not a git repository", result.Error ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_AcceptsChangedFilesOperation_ForGitRepository()
    {
        var workspace = CreateWorkspace(isGitRepo: true);
        var tool = new GitTool();
        var request = new ToolRequest(
            tool.Name,
            "changed-files",
            workspace,
            [ToolPermission.ReadOnly],
            CorrelationId: "corr-files");

        var result = await tool.ExecuteAsync(request, CancellationToken.None);

        Assert.DoesNotContain("Unsupported git operation", result.Error ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("not a git repository", result.Error ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_AcceptsPatchArtifactOperation_ForGitRepository()
    {
        var workspace = CreateWorkspace(isGitRepo: true);
        var tool = new GitTool();
        var request = new ToolRequest(
            tool.Name,
            "patch-artifact",
            workspace,
            [ToolPermission.ReadOnly],
            CorrelationId: "corr-patch");

        var result = await tool.ExecuteAsync(request, CancellationToken.None);

        Assert.DoesNotContain("Unsupported git operation", result.Error ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("not a git repository", result.Error ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_AcceptsCreateBranchOperation_ForGitRepository()
    {
        var workspace = CreateWorkspace(isGitRepo: true);
        var tool = new GitTool();
        var request = new ToolRequest(
            tool.Name,
            "create-branch:maaco/task-123-feature",
            workspace,
            [ToolPermission.ReadOnly],
            CorrelationId: "corr-create-branch");

        var result = await tool.ExecuteAsync(request, CancellationToken.None);

        Assert.DoesNotContain("Unsupported git operation", result.Error ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("not a git repository", result.Error ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsCreateBranchOperation_WithInvalidBranchName()
    {
        var workspace = CreateWorkspace(isGitRepo: true);
        var tool = new GitTool();
        var request = new ToolRequest(
            tool.Name,
            "create-branch:invalid name with spaces",
            workspace,
            [ToolPermission.ReadOnly],
            CorrelationId: "corr-create-branch-invalid");

        var result = await tool.ExecuteAsync(request, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("Unsupported git operation", result.Error ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_AcceptsCommitApprovedOperation_ForGitRepository()
    {
        var workspace = CreateWorkspace(isGitRepo: true);
        var tool = new GitTool();
        var request = new ToolRequest(
            tool.Name,
            "commit-approved:Apply MAACO changes",
            workspace,
            [ToolPermission.ReadOnly],
            CorrelationId: "corr-commit-approved");

        var result = await tool.ExecuteAsync(request, CancellationToken.None);

        Assert.DoesNotContain("Unsupported git operation", result.Error ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("not a git repository", result.Error ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsPlainCommitOperation_WithoutApprovalMarker()
    {
        var workspace = CreateWorkspace(isGitRepo: true);
        var tool = new GitTool();
        var request = new ToolRequest(
            tool.Name,
            "commit:Apply MAACO changes",
            workspace,
            [ToolPermission.ReadOnly],
            CorrelationId: "corr-commit-without-approval");

        var result = await tool.ExecuteAsync(request, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("Unsupported git operation", result.Error ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsPushOperation_InMvp()
    {
        var workspace = CreateWorkspace(isGitRepo: true);
        var tool = new GitTool();
        var request = new ToolRequest(
            tool.Name,
            "push origin main",
            workspace,
            [ToolPermission.ReadOnly],
            CorrelationId: "corr-push-disabled");

        var result = await tool.ExecuteAsync(request, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("disabled in MVP", result.Error ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_AcceptsRollbackUncommittedOperation_ForGitRepository()
    {
        var workspace = CreateWorkspace(isGitRepo: true);
        var tool = new GitTool();
        var request = new ToolRequest(
            tool.Name,
            "rollback-uncommitted",
            workspace,
            [ToolPermission.ReadOnly],
            CorrelationId: "corr-rollback");

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

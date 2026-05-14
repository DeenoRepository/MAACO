using MAACO.Core.Abstractions.Tools;
using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Domain.Entities;
using MAACO.Tools.Tools;
using System.Reflection;

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
    public async Task ExecuteAsync_RejectsResetHardOperation_WithoutApproval()
    {
        var workspace = CreateWorkspace(isGitRepo: true);
        var tool = new GitTool();
        var request = new ToolRequest(
            tool.Name,
            "reset --hard",
            workspace,
            [ToolPermission.ReadOnly],
            CorrelationId: "corr-reset-hard-disabled");

        var result = await tool.ExecuteAsync(request, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("requires approval", result.Error ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsResetHardWithRevision_WithoutApproval()
    {
        var workspace = CreateWorkspace(isGitRepo: true);
        var tool = new GitTool();
        var request = new ToolRequest(
            tool.Name,
            "reset --hard HEAD~1",
            workspace,
            [ToolPermission.ReadOnly],
            CorrelationId: "corr-reset-hard-rev-disabled");

        var result = await tool.ExecuteAsync(request, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("requires approval", result.Error ?? string.Empty, StringComparison.OrdinalIgnoreCase);
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
    }

    [Theory]
    [InlineData("src/MAACO.Tools/Tools/GitTool.cs", true)]
    [InlineData(".maaco/artifacts/patch-1.patch", true)]
    [InlineData("docs/notes.txt", false)]
    public void IsMaacoManagedPath_ReturnsExpectedValue(string path, bool expected)
    {
        var method = typeof(GitTool).GetMethod("IsMaacoManagedPath", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var actual = (bool)method!.Invoke(null, [path])!;
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ParseStatusPaths_UsesNewPathForRenames()
    {
        var method = typeof(GitTool).GetMethod("ParseStatusPaths", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var status = "R  old/file.txt -> src/MAACO.New/file.txt\n M src/MAACO.Core/Class1.cs\n";
        var paths = (IReadOnlyList<string>)method!.Invoke(null, [status])!;

        Assert.Contains("src/MAACO.New/file.txt", paths);
        Assert.Contains("src/MAACO.Core/Class1.cs", paths);
        Assert.DoesNotContain("old/file.txt", paths);
    }

    [Fact]
    public async Task ExecuteAsync_PersistsGitOperation_WhenTaskIdProvidedInJsonEnvelope()
    {
        var workspace = CreateWorkspace(isGitRepo: true);
        var repository = new InMemoryGitOperationRepository();
        var tool = new GitTool(repository);
        var taskId = Guid.NewGuid();
        var request = new ToolRequest(
            tool.Name,
            $$"""{"operation":"push origin main","taskId":"{{taskId}}"}""",
            workspace,
            [ToolPermission.ReadOnly],
            CorrelationId: "corr-gitop-persist");

        var result = await tool.ExecuteAsync(request, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Single(repository.Items);
        Assert.Equal(taskId, repository.Items[0].TaskId);
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

    private sealed class InMemoryGitOperationRepository : IGitOperationRepository
    {
        public List<GitOperation> Items { get; } = [];

        public Task AddAsync(GitOperation gitOperation, CancellationToken cancellationToken)
        {
            Items.Add(gitOperation);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}

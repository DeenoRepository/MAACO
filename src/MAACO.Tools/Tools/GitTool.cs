using MAACO.Core.Abstractions.Tools;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace MAACO.Tools.Tools;

public sealed class GitTool : IAgentTool
{
    private static readonly Regex BranchNameRegex = new(
        "^[a-zA-Z0-9._/-]+$",
        RegexOptions.Compiled);

    public string Name => "GitTool";

    public IReadOnlyCollection<ToolPermission> RequiredPermissions =>
    [
        ToolPermission.ReadOnly
    ];

    public async Task<ToolResult> ExecuteAsync(ToolRequest request, CancellationToken cancellationToken)
    {
        var startedAt = DateTimeOffset.UtcNow;
        var workingDirectory = Path.GetFullPath(request.WorkspacePath);
        if (!ToolPathSafety.IsWithinWorkspace(request.WorkspacePath, workingDirectory))
        {
            return Fail("Workspace boundary validation failed.", request.CorrelationId, startedAt);
        }

        if (!IsGitRepository(workingDirectory))
        {
            return Fail("Target path is not a git repository.", request.CorrelationId, startedAt);
        }

        var operation = ParseGitOperation(request.Input);
        if (operation is null)
        {
            return Fail("Unsupported git operation. Allowed: status, current-branch, branch, log, diff, changed-files, patch-artifact, create-branch:<name>, commit-approved:<message>, rollback-uncommitted.", request.CorrelationId, startedAt);
        }

        try
        {
            var (exitCode, stdOut, stdErr) = await RunProcessAsync(
                "git",
                operation.Command,
                workingDirectory,
                cancellationToken);

            string? artifactPath = null;
            if (exitCode == 0 && operation.GeneratesPatchArtifact)
            {
                artifactPath = SavePatchArtifact(workingDirectory, stdOut);
            }

            var output = JsonSerializer.Serialize(new
            {
                command = $"git {operation.Command}",
                operation = operation.Name,
                exitCode,
                stdout = Truncate(stdOut, 20000),
                stderr = Truncate(stdErr, 20000),
                artifactPath
            });

            return new ToolResult(
                Succeeded: exitCode == 0,
                Output: output,
                Error: exitCode == 0 ? null : "Git command failed.",
                Duration: DateTimeOffset.UtcNow - startedAt,
                CorrelationId: request.CorrelationId);
        }
        catch (Exception ex)
        {
            return Fail($"GitTool failed: {ex.Message}", request.CorrelationId, startedAt);
        }
    }

    private static GitOperationSpec? ParseGitOperation(string input)
    {
        var raw = input.Trim();
        var normalized = raw.ToLowerInvariant();

        if (normalized.StartsWith("create-branch:", StringComparison.Ordinal))
        {
            var branchName = raw["create-branch:".Length..].Trim();
            if (string.IsNullOrWhiteSpace(branchName) || !BranchNameRegex.IsMatch(branchName))
            {
                return null;
            }

            return new GitOperationSpec("create-branch", $"checkout -b {branchName}");
        }

        if (normalized.StartsWith("commit-approved:", StringComparison.Ordinal))
        {
            var commitMessage = raw["commit-approved:".Length..].Trim();
            if (string.IsNullOrWhiteSpace(commitMessage))
            {
                return null;
            }

            var escapedCommitMessage = commitMessage.Replace("\"", "\\\"", StringComparison.Ordinal);
            return new GitOperationSpec("commit-approved", $"commit -m \"{escapedCommitMessage}\"");
        }

        return normalized switch
        {
            "" => new GitOperationSpec("status", "status --short"),
            "status" => new GitOperationSpec("status", "status --short"),
            "current-branch" => new GitOperationSpec("current-branch", "branch --show-current"),
            "branch" => new GitOperationSpec("branch", "branch --show-current"),
            "log" => new GitOperationSpec("log", "log --oneline -n 20"),
            "diff" => new GitOperationSpec("diff", "diff -- ."),
            "changed-files" => new GitOperationSpec("changed-files", "status --short --untracked-files=all"),
            "patch-artifact" => new GitOperationSpec("patch-artifact", "diff -- .", GeneratesPatchArtifact: true),
            "rollback-uncommitted" => new GitOperationSpec("rollback-uncommitted", "restore --staged --worktree -- ."),
            _ => null
        };
    }

    private static async Task<(int ExitCode, string StdOut, string StdErr)> RunProcessAsync(
        string fileName,
        string arguments,
        string workingDirectory,
        CancellationToken cancellationToken)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        var stdOutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stdErrTask = process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        return (process.ExitCode, await stdOutTask, await stdErrTask);
    }

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..max];

    private static bool IsGitRepository(string workingDirectory)
    {
        var gitPath = Path.Combine(workingDirectory, ".git");
        return Directory.Exists(gitPath) || File.Exists(gitPath);
    }

    private static string SavePatchArtifact(string workingDirectory, string patchContent)
    {
        var artifactsDirectory = Path.Combine(workingDirectory, ".maaco", "artifacts");
        Directory.CreateDirectory(artifactsDirectory);
        var fileName = $"patch-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}.patch";
        var fullPath = Path.Combine(artifactsDirectory, fileName);
        File.WriteAllText(fullPath, patchContent);
        return fullPath;
    }

    private static ToolResult Fail(string error, string? correlationId, DateTimeOffset startedAt) =>
        new(
            Succeeded: false,
            Output: string.Empty,
            Error: error,
            Duration: DateTimeOffset.UtcNow - startedAt,
            CorrelationId: correlationId);

    private sealed record GitOperationSpec(
        string Name,
        string Command,
        bool GeneratesPatchArtifact = false);
}

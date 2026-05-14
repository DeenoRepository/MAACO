using MAACO.Core.Abstractions.Tools;
using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Domain.Entities;
using MAACO.Core.Domain.Enums;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MAACO.Tools.Tools;

public sealed class GitTool(IGitOperationRepository? gitOperationRepository = null) : IAgentTool
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
        var (operationInput, taskId) = ParseOperationInputAndTaskId(request.Input);
        var workingDirectory = Path.GetFullPath(request.WorkspacePath);
        if (!ToolPathSafety.IsWithinWorkspace(request.WorkspacePath, workingDirectory))
        {
            return Fail("Workspace boundary validation failed.", request.CorrelationId, startedAt);
        }

        if (!IsGitRepository(workingDirectory))
        {
            return Fail("Target path is not a git repository.", request.CorrelationId, startedAt);
        }

        var forbiddenReason = GetForbiddenOperationReason(operationInput);
        if (forbiddenReason is not null)
        {
            var forbiddenResult = Fail(forbiddenReason, request.CorrelationId, startedAt);
            await PersistGitOperationAsync(taskId, operationInput, forbiddenResult, cancellationToken);
            return forbiddenResult;
        }

        var operation = ParseGitOperation(operationInput);
        if (operation is null)
        {
            var unsupportedResult = Fail("Unsupported git operation. Allowed: status, current-branch, branch, log, diff, changed-files, patch-artifact, create-branch:<name>, commit-approved:<message>, rollback-uncommitted.", request.CorrelationId, startedAt);
            await PersistGitOperationAsync(taskId, operationInput, unsupportedResult, cancellationToken);
            return unsupportedResult;
        }

        try
        {
            if (operation.Name == "rollback-uncommitted")
            {
                var rollbackResult = await ExecuteRollbackUncommittedAsync(
                    request,
                    startedAt,
                    workingDirectory,
                    cancellationToken);
                await PersistGitOperationAsync(taskId, operation.Name, rollbackResult, cancellationToken);
                return rollbackResult;
            }

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

            var result = new ToolResult(
                Succeeded: exitCode == 0,
                Output: output,
                Error: exitCode == 0 ? null : "Git command failed.",
                Duration: DateTimeOffset.UtcNow - startedAt,
                CorrelationId: request.CorrelationId);
            await PersistGitOperationAsync(taskId, operation.Name, result, cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            var failure = Fail($"GitTool failed: {ex.Message}", request.CorrelationId, startedAt);
            await PersistGitOperationAsync(taskId, operationInput, failure, cancellationToken);
            return failure;
        }
    }

    private static (string OperationInput, Guid? TaskId) ParseOperationInputAndTaskId(string input)
    {
        var raw = input?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return (string.Empty, null);
        }

        try
        {
            using var document = JsonDocument.Parse(raw);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return (raw, null);
            }

            var root = document.RootElement;
            if (!root.TryGetProperty("operation", out var operationProperty) ||
                operationProperty.ValueKind != JsonValueKind.String)
            {
                return (raw, null);
            }

            var operation = operationProperty.GetString() ?? string.Empty;
            Guid? taskId = null;
            if (root.TryGetProperty("taskId", out var taskIdProperty) &&
                taskIdProperty.ValueKind == JsonValueKind.String &&
                Guid.TryParse(taskIdProperty.GetString(), out var parsedTaskId))
            {
                taskId = parsedTaskId;
            }

            return (operation.Trim(), taskId);
        }
        catch
        {
            return (raw, null);
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

    private static string? GetForbiddenOperationReason(string input)
    {
        var normalized = input.Trim().ToLowerInvariant();
        if (normalized == "push"
            || normalized.StartsWith("push ", StringComparison.Ordinal)
            || normalized.StartsWith("push:", StringComparison.Ordinal))
        {
            return "Git push operations are disabled in MVP.";
        }

        if (normalized == "reset --hard"
            || normalized.StartsWith("reset --hard ", StringComparison.Ordinal)
            || normalized.StartsWith("reset --hard:", StringComparison.Ordinal))
        {
            return "Git force reset (--hard) requires approval and is disabled by default.";
        }

        return null;
    }

    private async Task PersistGitOperationAsync(
        Guid? taskId,
        string operationInput,
        ToolResult result,
        CancellationToken cancellationToken)
    {
        if (gitOperationRepository is null || taskId is null || taskId == Guid.Empty)
        {
            return;
        }

        try
        {
            var gitOperation = new GitOperation
            {
                TaskId = taskId.Value,
                Type = MapGitOperationType(operationInput),
                Succeeded = result.Succeeded,
                Details = Truncate(string.IsNullOrWhiteSpace(result.Error) ? result.Output : result.Error!, 2000),
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await gitOperationRepository.AddAsync(gitOperation, cancellationToken);
            await gitOperationRepository.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // Persistence failure should not fail git command execution in MVP.
        }
    }

    private static GitOperationType MapGitOperationType(string operationInput)
    {
        var normalized = operationInput.Trim().ToLowerInvariant();
        if (normalized.StartsWith("create-branch:", StringComparison.Ordinal) ||
            normalized == "current-branch" ||
            normalized == "branch")
        {
            return GitOperationType.Branch;
        }

        if (normalized.StartsWith("commit-approved:", StringComparison.Ordinal))
        {
            return GitOperationType.Commit;
        }

        if (normalized == "rollback-uncommitted")
        {
            return GitOperationType.Rollback;
        }

        if (normalized == "diff" || normalized == "patch-artifact")
        {
            return GitOperationType.Diff;
        }

        return GitOperationType.Status;
    }

    private static async Task<ToolResult> ExecuteRollbackUncommittedAsync(
        ToolRequest request,
        DateTimeOffset startedAt,
        string workingDirectory,
        CancellationToken cancellationToken)
    {
        var (statusExitCode, statusStdOut, statusStdErr) = await RunProcessAsync(
            "git",
            "status --porcelain --untracked-files=all",
            workingDirectory,
            cancellationToken);

        if (statusExitCode != 0)
        {
            return Fail(
                $"Git command failed: status --porcelain --untracked-files=all. {Truncate(statusStdErr, 20000)}",
                request.CorrelationId,
                startedAt);
        }

        var changedFiles = ParseStatusPaths(statusStdOut);
        var nonMaacoFiles = changedFiles
            .Where(path => !IsMaacoManagedPath(path))
            .ToArray();

        if (nonMaacoFiles.Length > 0)
        {
            return Fail(
                $"Rollback blocked: found non-MAACO changes ({string.Join(", ", nonMaacoFiles)}).",
                request.CorrelationId,
                startedAt);
        }

        var maacoFiles = changedFiles
            .Where(IsMaacoManagedPath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (maacoFiles.Length == 0)
        {
            return new ToolResult(
                Succeeded: true,
                Output: JsonSerializer.Serialize(new
                {
                    command = "git status --porcelain --untracked-files=all",
                    operation = "rollback-uncommitted",
                    restoredFiles = Array.Empty<string>(),
                    cleanedFiles = Array.Empty<string>(),
                    skipped = true,
                    reason = "No MAACO-managed changes found."
                }),
                Error: null,
                Duration: DateTimeOffset.UtcNow - startedAt,
                CorrelationId: request.CorrelationId);
        }

        var quotedFiles = string.Join(" ", maacoFiles.Select(QuoteGitPath));

        var (restoreExitCode, restoreStdOut, restoreStdErr) = await RunProcessAsync(
            "git",
            $"restore --staged --worktree -- {quotedFiles}",
            workingDirectory,
            cancellationToken);

        if (restoreExitCode != 0)
        {
            return Fail(
                $"Git command failed: restore --staged --worktree. {Truncate(restoreStdErr, 20000)}",
                request.CorrelationId,
                startedAt);
        }

        var untrackedFiles = ParseUntrackedPaths(statusStdOut)
            .Where(IsMaacoManagedPath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var cleanedFiles = Array.Empty<string>();
        if (untrackedFiles.Length > 0)
        {
            var quotedUntracked = string.Join(" ", untrackedFiles.Select(QuoteGitPath));
            var (cleanExitCode, _, cleanStdErr) = await RunProcessAsync(
                "git",
                $"clean -f -- {quotedUntracked}",
                workingDirectory,
                cancellationToken);

            if (cleanExitCode != 0)
            {
                return Fail(
                    $"Git command failed: clean -f. {Truncate(cleanStdErr, 20000)}",
                    request.CorrelationId,
                    startedAt);
            }

            cleanedFiles = untrackedFiles;
        }

        var output = JsonSerializer.Serialize(new
        {
            command = $"git restore --staged --worktree -- {quotedFiles}",
            operation = "rollback-uncommitted",
            exitCode = restoreExitCode,
            stdout = Truncate(restoreStdOut, 20000),
            stderr = Truncate(restoreStdErr, 20000),
            restoredFiles = maacoFiles,
            cleanedFiles
        });

        return new ToolResult(
            Succeeded: true,
            Output: output,
            Error: null,
            Duration: DateTimeOffset.UtcNow - startedAt,
            CorrelationId: request.CorrelationId);
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

    private static IReadOnlyList<string> ParseStatusPaths(string porcelain)
    {
        var paths = new List<string>();
        var lines = porcelain.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            if (line.Length < 4)
            {
                continue;
            }

            var pathSection = line[3..].Trim();
            var renameParts = pathSection.Split(" -> ", StringSplitOptions.None);
            var candidate = renameParts.Length > 1 ? renameParts[^1] : pathSection;
            if (!string.IsNullOrWhiteSpace(candidate))
            {
                paths.Add(candidate.Trim());
            }
        }

        return paths;
    }

    private static IReadOnlyList<string> ParseUntrackedPaths(string porcelain)
    {
        var paths = new List<string>();
        var lines = porcelain.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            if (!line.StartsWith("?? ", StringComparison.Ordinal) || line.Length < 4)
            {
                continue;
            }

            paths.Add(line[3..].Trim());
        }

        return paths;
    }

    private static string QuoteGitPath(string path) =>
        $"\"{path.Replace("\"", "\\\"", StringComparison.Ordinal)}\"";

    private static bool IsMaacoManagedPath(string relativePath)
    {
        var normalized = relativePath.Replace('\\', '/');
        return normalized.Contains("maaco", StringComparison.OrdinalIgnoreCase)
            || normalized.StartsWith(".maaco/", StringComparison.OrdinalIgnoreCase);
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

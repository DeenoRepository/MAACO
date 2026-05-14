using MAACO.Core.Abstractions.Sandbox;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace MAACO.Sandbox;

public sealed class LocalSandboxExecutor : ISandboxExecutor
{
    private static readonly HashSet<string> AllowedCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        "dotnet",
        "cmd.exe",
        "powershell.exe",
        "pwsh.exe",
        "npm",
        "node",
        "python",
        "python.exe",
        "pytest"
    };

    private static readonly string[] DangerousArgumentTokens =
    [
        "rm -rf /",
        "sudo",
        "format",
        "mkfs",
        "del /s",
        "curl | sh",
        "wget | sh",
        "shutdown",
        "reboot"
    ];

    private static readonly string[] BlockedPathFragments =
    [
        "\\.ssh",
        "/.ssh",
        "\\.aws",
        "/.aws"
    ];

    private static readonly string[] BlockedSystemPathPrefixes =
    [
        "C:\\Windows",
        "C:\\Program Files",
        "C:\\Program Files (x86)",
        "C:\\ProgramData"
    ];

    private static readonly Regex SensitiveAssignmentRegex = new(
        @"\b(api[_-]?key|token|access[_-]?token|password|secret)\b\s*=\s*([^\s]+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public async Task<SandboxResult> ExecuteAsync(SandboxRequest request, CancellationToken cancellationToken)
    {
        var startedAt = DateTimeOffset.UtcNow;
        try
        {
            var workspacePath = Path.GetFullPath(request.WorkspacePath);
            if (IsBlockedPath(workspacePath))
            {
                return new SandboxResult(
                    Succeeded: false,
                    ExitCode: -1,
                    StdOut: string.Empty,
                    StdErr: string.Empty,
                    Duration: DateTimeOffset.UtcNow - startedAt,
                    Error: "Workspace path is blocked by sandbox policy.");
            }

            if (!IsAllowedCommand(request.FileName))
            {
                return new SandboxResult(
                    Succeeded: false,
                    ExitCode: -1,
                    StdOut: string.Empty,
                    StdErr: string.Empty,
                    Duration: DateTimeOffset.UtcNow - startedAt,
                    Error: "Command is not in sandbox allowlist.");
            }

            if (ContainsDangerousArguments(request.Arguments))
            {
                return new SandboxResult(
                    Succeeded: false,
                    ExitCode: -1,
                    StdOut: string.Empty,
                    StdErr: string.Empty,
                    Duration: DateTimeOffset.UtcNow - startedAt,
                    Error: "Command contains blocked dangerous pattern.");
            }

            var workingDirectory = ResolveWorkingDirectory(workspacePath, request.Options.WorkingDirectory);
            if (IsBlockedPath(workingDirectory))
            {
                return new SandboxResult(
                    Succeeded: false,
                    ExitCode: -1,
                    StdOut: string.Empty,
                    StdErr: string.Empty,
                    Duration: DateTimeOffset.UtcNow - startedAt,
                    Error: "Working directory path is blocked by sandbox policy.");
            }

            if (!IsWithinWorkspace(workspacePath, workingDirectory))
            {
                return new SandboxResult(
                    Succeeded: false,
                    ExitCode: -1,
                    StdOut: string.Empty,
                    StdErr: string.Empty,
                    Duration: DateTimeOffset.UtcNow - startedAt,
                    Error: "Working directory is outside workspace.");
            }

            using var timeoutCts = request.Options.Timeout > TimeSpan.Zero
                ? new CancellationTokenSource(request.Options.Timeout)
                : null;
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                timeoutCts?.Token ?? CancellationToken.None);

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = request.FileName,
                    Arguments = request.Arguments,
                    WorkingDirectory = workingDirectory,
                    RedirectStandardOutput = request.Options.CaptureStdOut,
                    RedirectStandardError = request.Options.CaptureStdErr,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            if (ContainsBlockedEnvironmentPath(request.Options.EnvironmentVariables))
            {
                return new SandboxResult(
                    Succeeded: false,
                    ExitCode: -1,
                    StdOut: string.Empty,
                    StdErr: string.Empty,
                    Duration: DateTimeOffset.UtcNow - startedAt,
                    Error: "Environment variable contains blocked path.");
            }

            ApplyEnvironmentVariables(process.StartInfo, request.Options.EnvironmentVariables);
            process.Start();

            var stdoutTask = request.Options.CaptureStdOut
                ? process.StandardOutput.ReadToEndAsync(linkedCts.Token)
                : Task.FromResult(string.Empty);
            var stderrTask = request.Options.CaptureStdErr
                ? process.StandardError.ReadToEndAsync(linkedCts.Token)
                : Task.FromResult(string.Empty);

            await process.WaitForExitAsync(linkedCts.Token);

            var stdout = await stdoutTask;
            var stderr = await stderrTask;
            var redactedStdOut = RedactEnvValues(stdout, request.Options.EnvironmentVariables);
            var redactedStdErr = RedactEnvValues(stderr, request.Options.EnvironmentVariables);
            return new SandboxResult(
                Succeeded: process.ExitCode == 0,
                ExitCode: process.ExitCode,
                StdOut: redactedStdOut,
                StdErr: redactedStdErr,
                Duration: DateTimeOffset.UtcNow - startedAt);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new SandboxResult(
                Succeeded: false,
                ExitCode: -1,
                StdOut: string.Empty,
                StdErr: string.Empty,
                Duration: DateTimeOffset.UtcNow - startedAt,
                TimedOut: true,
                Error: "Sandbox execution timed out.");
        }
        catch (OperationCanceledException)
        {
            return new SandboxResult(
                Succeeded: false,
                ExitCode: -1,
                StdOut: string.Empty,
                StdErr: string.Empty,
                Duration: DateTimeOffset.UtcNow - startedAt,
                Cancelled: true,
                Error: "Sandbox execution cancelled.");
        }
        catch (Exception ex)
        {
            return new SandboxResult(
                Succeeded: false,
                ExitCode: -1,
                StdOut: string.Empty,
                StdErr: string.Empty,
                Duration: DateTimeOffset.UtcNow - startedAt,
                Error: $"Sandbox execution failed: {ex.Message}");
        }
    }

    private static string ResolveWorkingDirectory(string workspacePath, string? workingDirectory)
    {
        if (string.IsNullOrWhiteSpace(workingDirectory))
        {
            return workspacePath;
        }

        return Path.GetFullPath(Path.IsPathRooted(workingDirectory)
            ? workingDirectory
            : Path.Combine(workspacePath, workingDirectory));
    }

    private static bool IsWithinWorkspace(string workspacePath, string targetPath)
    {
        var workspaceFull = Path.GetFullPath(workspacePath);
        var targetFull = Path.GetFullPath(targetPath);

        if (string.Equals(workspaceFull, targetFull, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!workspaceFull.EndsWith(Path.DirectorySeparatorChar))
        {
            workspaceFull += Path.DirectorySeparatorChar;
        }

        return targetFull.StartsWith(workspaceFull, StringComparison.OrdinalIgnoreCase);
    }

    private static void ApplyEnvironmentVariables(
        ProcessStartInfo startInfo,
        IReadOnlyDictionary<string, string>? environmentVariables)
    {
        if (environmentVariables is null || environmentVariables.Count == 0)
        {
            return;
        }

        foreach (var (key, value) in environmentVariables)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            startInfo.Environment[key] = value ?? string.Empty;
        }
    }

    private static bool IsAllowedCommand(string fileName)
    {
        var normalized = fileName.Trim();
        var command = Path.GetFileName(normalized);
        return AllowedCommands.Contains(command);
    }

    private static bool ContainsDangerousArguments(string arguments)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            return false;
        }

        return DangerousArgumentTokens.Any(token =>
            arguments.Contains(token, StringComparison.OrdinalIgnoreCase));
    }

    private static bool ContainsBlockedEnvironmentPath(IReadOnlyDictionary<string, string>? environmentVariables)
    {
        if (environmentVariables is null || environmentVariables.Count == 0)
        {
            return false;
        }

        foreach (var value in environmentVariables.Values)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            if (IsBlockedPath(value))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsBlockedPath(string path)
    {
        var normalized = path.Replace('/', '\\');

        if (BlockedPathFragments.Any(fragment =>
                normalized.Contains(fragment.Replace('/', '\\'), StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        return BlockedSystemPathPrefixes.Any(prefix =>
            normalized.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    private static string RedactEnvValues(
        string text,
        IReadOnlyDictionary<string, string>? environmentVariables)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        var result = SensitiveAssignmentRegex.Replace(text, m => $"{m.Groups[1].Value}=***REDACTED***");

        if (environmentVariables is null || environmentVariables.Count == 0)
        {
            return result;
        }

        foreach (var value in environmentVariables.Values)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            result = result.Replace(value, "***REDACTED***", StringComparison.Ordinal);
        }

        return result;
    }
}

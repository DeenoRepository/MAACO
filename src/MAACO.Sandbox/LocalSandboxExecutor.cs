using MAACO.Core.Abstractions.Sandbox;
using System.Diagnostics;

namespace MAACO.Sandbox;

public sealed class LocalSandboxExecutor : ISandboxExecutor
{
    public async Task<SandboxResult> ExecuteAsync(SandboxRequest request, CancellationToken cancellationToken)
    {
        var startedAt = DateTimeOffset.UtcNow;
        try
        {
            var workspacePath = Path.GetFullPath(request.WorkspacePath);
            var workingDirectory = ResolveWorkingDirectory(workspacePath, request.Options.WorkingDirectory);
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
            return new SandboxResult(
                Succeeded: process.ExitCode == 0,
                ExitCode: process.ExitCode,
                StdOut: stdout,
                StdErr: stderr,
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
}

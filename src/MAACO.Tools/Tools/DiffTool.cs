using MAACO.Core.Abstractions.Tools;
using System.Diagnostics;
using System.Text.Json;

namespace MAACO.Tools.Tools;

public sealed class DiffTool : IAgentTool
{
    public string Name => "DiffTool";

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

        try
        {
            var (exitCode, stdOut, stdErr) = await RunProcessAsync(
                "git",
                "diff -- .",
                workingDirectory,
                cancellationToken);

            var output = JsonSerializer.Serialize(new
            {
                command = "git diff -- .",
                exitCode,
                diff = Truncate(stdOut, 50000),
                stderr = Truncate(stdErr, 20000),
                truncated = stdOut.Length > 50000
            });

            return new ToolResult(
                Succeeded: exitCode == 0,
                Output: output,
                Error: exitCode == 0 ? null : "Diff command failed.",
                Duration: DateTimeOffset.UtcNow - startedAt,
                CorrelationId: request.CorrelationId);
        }
        catch (Exception ex)
        {
            return Fail($"DiffTool failed: {ex.Message}", request.CorrelationId, startedAt);
        }
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

    private static ToolResult Fail(string error, string? correlationId, DateTimeOffset startedAt) =>
        new(
            Succeeded: false,
            Output: string.Empty,
            Error: error,
            Duration: DateTimeOffset.UtcNow - startedAt,
            CorrelationId: correlationId);
}

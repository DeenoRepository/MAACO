using MAACO.Core.Abstractions.Tools;
using System.Diagnostics;
using System.Text.Json;

namespace MAACO.Tools.Tools;

public sealed class TestTool : IAgentTool
{
    public string Name => "TestTool";

    public IReadOnlyCollection<ToolPermission> RequiredPermissions =>
    [
        ToolPermission.ExecuteCommand,
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

        var command = "dotnet";
        var arguments = "test --nologo --verbosity minimal";

        try
        {
            var (exitCode, stdOut, stdErr) = await RunProcessAsync(
                command,
                arguments,
                workingDirectory,
                cancellationToken);

            var output = JsonSerializer.Serialize(new
            {
                command = $"{command} {arguments}",
                exitCode,
                stdout = Truncate(stdOut, 20000),
                stderr = Truncate(stdErr, 20000)
            });

            return new ToolResult(
                Succeeded: exitCode == 0,
                Output: output,
                Error: exitCode == 0 ? null : "Test command failed.",
                Duration: DateTimeOffset.UtcNow - startedAt,
                CorrelationId: request.CorrelationId);
        }
        catch (OperationCanceledException)
        {
            return new ToolResult(
                Succeeded: false,
                Output: string.Empty,
                Error: "Tests cancelled.",
                Duration: DateTimeOffset.UtcNow - startedAt,
                CorrelationId: request.CorrelationId);
        }
        catch (Exception ex)
        {
            return Fail($"TestTool failed: {ex.Message}", request.CorrelationId, startedAt);
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

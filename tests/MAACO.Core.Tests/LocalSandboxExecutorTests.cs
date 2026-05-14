using MAACO.Core.Abstractions.Sandbox;
using MAACO.Sandbox;

namespace MAACO.Core.Tests;

public sealed class LocalSandboxExecutorTests
{
    private const string CmdExe = "C:\\Windows\\System32\\cmd.exe";

    [Fact]
    public async Task ExecuteAsync_CapturesStdOutStdErrAndExitCode()
    {
        var workspace = CreateWorkspace();
        var executor = new LocalSandboxExecutor();
        var request = new SandboxRequest(
            FileName: CmdExe,
            Arguments: "/c \"echo hello & echo oops 1>&2 & exit /b 7\"",
            WorkspacePath: workspace,
            Options: new SandboxOptions(TimeSpan.FromSeconds(10)));

        var result = await executor.ExecuteAsync(request, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(7, result.ExitCode);
        Assert.Contains("hello", result.StdOut, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("oops", result.StdErr, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_TimesOut_WhenTimeoutExceeded()
    {
        var workspace = CreateWorkspace();
        var executor = new LocalSandboxExecutor();
        var request = new SandboxRequest(
            FileName: CmdExe,
            Arguments: "/c \"ping 127.0.0.1 -n 6 > nul\"",
            WorkspacePath: workspace,
            Options: new SandboxOptions(TimeSpan.FromMilliseconds(100)));

        var result = await executor.ExecuteAsync(request, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.True(result.TimedOut);
    }

    [Fact]
    public async Task ExecuteAsync_Cancelled_WhenCancellationRequested()
    {
        var workspace = CreateWorkspace();
        var executor = new LocalSandboxExecutor();
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(100);
        var request = new SandboxRequest(
            FileName: CmdExe,
            Arguments: "/c \"ping 127.0.0.1 -n 6 > nul\"",
            WorkspacePath: workspace,
            Options: new SandboxOptions(TimeSpan.FromSeconds(10)));

        var result = await executor.ExecuteAsync(request, cts.Token);

        Assert.False(result.Succeeded);
        Assert.True(result.Cancelled);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsWorkingDirectoryOutsideWorkspace()
    {
        var workspace = CreateWorkspace();
        var outsidePath = Path.GetTempPath();
        var executor = new LocalSandboxExecutor();
        var request = new SandboxRequest(
            FileName: CmdExe,
            Arguments: "/c \"echo hello\"",
            WorkspacePath: workspace,
            Options: new SandboxOptions(TimeSpan.FromSeconds(10), WorkingDirectory: outsidePath));

        var result = await executor.ExecuteAsync(request, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("outside workspace", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    private static string CreateWorkspace()
    {
        var path = Path.Combine(Path.GetTempPath(), "maaco-sandbox-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}

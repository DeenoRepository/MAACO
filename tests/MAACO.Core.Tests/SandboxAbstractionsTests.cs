using MAACO.Core.Abstractions.Sandbox;

namespace MAACO.Core.Tests;

public sealed class SandboxAbstractionsTests
{
    [Fact]
    public void SandboxRequest_HoldsCommandAndOptions()
    {
        var options = new SandboxOptions(
            Timeout: TimeSpan.FromSeconds(30),
            WorkingDirectory: "src",
            EnvironmentVariables: new Dictionary<string, string> { ["DOTNET_ENVIRONMENT"] = "Development" });

        var request = new SandboxRequest(
            FileName: "dotnet",
            Arguments: "build",
            WorkspacePath: "D:\\Projects\\MAACO",
            Options: options);

        Assert.Equal("dotnet", request.FileName);
        Assert.Equal("build", request.Arguments);
        Assert.Equal("D:\\Projects\\MAACO", request.WorkspacePath);
        Assert.Equal(TimeSpan.FromSeconds(30), request.Options.Timeout);
    }

    [Fact]
    public void SandboxResult_RepresentsCompletedExecution()
    {
        var result = new SandboxResult(
            Succeeded: true,
            ExitCode: 0,
            StdOut: "ok",
            StdErr: string.Empty,
            Duration: TimeSpan.FromMilliseconds(120));

        Assert.True(result.Succeeded);
        Assert.Equal(0, result.ExitCode);
        Assert.False(result.TimedOut);
        Assert.False(result.Cancelled);
        Assert.Null(result.Error);
    }
}

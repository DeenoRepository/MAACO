using MAACO.Core.Abstractions.Sandbox;

namespace MAACO.Sandbox;

public sealed class DockerSandboxExecutorStub : IDockerSandboxExecutor
{
    public Task<SandboxResult> ExecuteAsync(SandboxRequest request, CancellationToken cancellationToken)
    {
        var result = new SandboxResult(
            Succeeded: false,
            ExitCode: -1,
            StdOut: string.Empty,
            StdErr: string.Empty,
            Duration: TimeSpan.Zero,
            Error: "Docker sandbox is not implemented yet.");

        return Task.FromResult(result);
    }
}

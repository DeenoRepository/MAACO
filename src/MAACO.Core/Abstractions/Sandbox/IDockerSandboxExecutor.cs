namespace MAACO.Core.Abstractions.Sandbox;

public interface IDockerSandboxExecutor
{
    Task<SandboxResult> ExecuteAsync(SandboxRequest request, CancellationToken cancellationToken);
}

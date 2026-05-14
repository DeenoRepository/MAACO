namespace MAACO.Core.Abstractions.Sandbox;

public interface ISandboxExecutor
{
    Task<SandboxResult> ExecuteAsync(SandboxRequest request, CancellationToken cancellationToken);
}

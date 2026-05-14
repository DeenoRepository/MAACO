using MAACO.Core.Abstractions.Sandbox;

namespace MAACO.Sandbox;

public sealed class LocalSandboxExecutor : ISandboxExecutor
{
    public Task<SandboxResult> ExecuteAsync(SandboxRequest request, CancellationToken cancellationToken) =>
        throw new NotImplementedException("LocalSandboxExecutor is not implemented yet.");
}

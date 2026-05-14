using MAACO.Core.Abstractions.Sandbox;
using MAACO.Sandbox;
using Microsoft.Extensions.DependencyInjection;

namespace MAACO.Core.Tests;

public sealed class SandboxDependencyInjectionTests
{
    [Fact]
    public void AddMaacoSandbox_RegistersLocalAndDockerExecutors()
    {
        var services = new ServiceCollection();
        services.AddMaacoSandbox();
        using var provider = services.BuildServiceProvider();

        var local = provider.GetService<ISandboxExecutor>();
        var docker = provider.GetService<IDockerSandboxExecutor>();

        Assert.NotNull(local);
        Assert.NotNull(docker);
    }
}

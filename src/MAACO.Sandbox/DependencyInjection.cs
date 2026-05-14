using MAACO.Core.Abstractions.Sandbox;
using Microsoft.Extensions.DependencyInjection;

namespace MAACO.Sandbox;

public static class DependencyInjection
{
    public static IServiceCollection AddMaacoSandbox(this IServiceCollection services)
    {
        services.AddScoped<ISandboxExecutor, LocalSandboxExecutor>();
        return services;
    }
}

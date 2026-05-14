using MAACO.Core.Abstractions.Tools;
using Microsoft.Extensions.DependencyInjection;
using MAACO.Tools.Tools;

namespace MAACO.Tools;

public static class DependencyInjection
{
    public static IServiceCollection AddMaacoTools(this IServiceCollection services)
    {
        services.AddSingleton<IAgentTool, FileSystemTool>();
        services.AddSingleton<IAgentTool, ProjectScannerTool>();
        services.AddSingleton<IAgentTool, BuildTool>();
        services.AddSingleton<IAgentTool, TestTool>();
        services.AddSingleton<IToolRegistry, ToolRegistry>();
        return services;
    }
}

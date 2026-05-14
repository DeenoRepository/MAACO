using MAACO.Core.Abstractions.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace MAACO.Tools;

public static class DependencyInjection
{
    public static IServiceCollection AddMaacoTools(this IServiceCollection services)
    {
        services.AddSingleton<IToolRegistry, ToolRegistry>();
        return services;
    }
}

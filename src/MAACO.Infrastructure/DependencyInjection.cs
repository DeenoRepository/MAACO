using MAACO.Core.Abstractions.Events;
using MAACO.Infrastructure.Events;
using Microsoft.Extensions.DependencyInjection;

namespace MAACO.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddMaacoInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IEventBus, InMemoryEventBus>();
        return services;
    }
}

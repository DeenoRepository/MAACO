using MAACO.Agents.Abstractions;
using MAACO.Agents.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MAACO.Agents;

public static class DependencyInjection
{
    public static IServiceCollection AddMaacoAgents(this IServiceCollection services)
    {
        services.AddSingleton<IAgentRegistry, AgentRegistry>();
        services.AddScoped<IAgentExecutionService, AgentExecutionService>();
        return services;
    }
}

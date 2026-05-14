using MAACO.Agents.Abstractions;
using MAACO.Agents.Agents;
using MAACO.Agents.Prompts;
using MAACO.Agents.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MAACO.Agents;

public static class DependencyInjection
{
    public static IServiceCollection AddMaacoAgents(this IServiceCollection services)
    {
        services.AddSingleton<IAgentPromptCatalog, DefaultAgentPromptCatalog>();
        services.AddSingleton<IAgent, OrchestratorAgent>();
        services.AddSingleton<IAgent, TaskPlannerAgent>();
        services.AddSingleton<IAgent, BackendDeveloperAgent>();
        services.AddSingleton<IAgent, TestWriterAgent>();
        services.AddSingleton<IAgent, DebuggingAgent>();
        services.AddSingleton<IAgent, GitManagerAgent>();
        services.AddSingleton<IAgent, DocumentationAgent>();
        services.AddSingleton<IAgentRegistry, AgentRegistry>();
        services.AddScoped<IAgentExecutionService, AgentExecutionService>();
        return services;
    }
}

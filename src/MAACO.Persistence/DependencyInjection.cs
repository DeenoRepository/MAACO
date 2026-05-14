using MAACO.Core.Abstractions.Repositories;
using MAACO.Persistence.Data;
using MAACO.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MAACO.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddMaacoPersistence(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<MaacoDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IWorkflowRepository, WorkflowRepository>();
        services.AddScoped<ILogRepository, LogRepository>();
        services.AddScoped<IMemoryRepository, MemoryRepository>();

        return services;
    }
}

using MAACO.App.Services;
using MAACO.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace MAACO.App.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMaacoDesktopShell(this IServiceCollection services)
    {
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IApiClient, ApiClient>();
        services.AddSingleton<IRealtimeClient, RealtimeClient>();
        services.AddSingleton<IProjectsClient, ProjectsClient>();

        services.AddHttpClient<IApiClient, ApiClient>(client =>
        {
            client.BaseAddress = new Uri("http://localhost:5168/");
            client.Timeout = TimeSpan.FromSeconds(15);
        });
        services.AddHttpClient<IProjectsClient, ProjectsClient>(client =>
        {
            client.BaseAddress = new Uri("http://localhost:5168/");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddSingleton<DashboardViewModel>();
        services.AddSingleton<ProjectsViewModel>();
        services.AddSingleton<TaskCreationViewModel>();
        services.AddSingleton<WorkflowMonitorViewModel>();
        services.AddSingleton<DiffReviewViewModel>();
        services.AddSingleton<SettingsViewModel>();
        services.AddSingleton<LogsViewModel>();
        services.AddSingleton<MainWindowViewModel>();

        return services;
    }
}

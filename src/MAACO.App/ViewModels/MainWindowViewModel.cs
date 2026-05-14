using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAACO.App.Services;

namespace MAACO.App.ViewModels;

public sealed partial class MainWindowViewModel : BaseViewModel
{
    private readonly INavigationService navigationService;
    private readonly IApiClient apiClient;
    private readonly IRealtimeClient realtimeClient;
    private readonly DashboardViewModel dashboardViewModel;
    private readonly ProjectsViewModel projectsViewModel;
    private readonly TaskCreationViewModel taskCreationViewModel;
    private readonly WorkflowMonitorViewModel workflowMonitorViewModel;
    private readonly DiffReviewViewModel diffReviewViewModel;
    private readonly SettingsViewModel settingsViewModel;
    private readonly LogsViewModel logsViewModel;

    [ObservableProperty]
    private BaseViewModel currentView;

    [ObservableProperty]
    private string statusText = "Initializing...";

    [ObservableProperty]
    private bool isBusy;

    public MainWindowViewModel(
        INavigationService navigationService,
        IApiClient apiClient,
        IRealtimeClient realtimeClient,
        DashboardViewModel dashboardViewModel,
        ProjectsViewModel projectsViewModel,
        TaskCreationViewModel taskCreationViewModel,
        WorkflowMonitorViewModel workflowMonitorViewModel,
        DiffReviewViewModel diffReviewViewModel,
        SettingsViewModel settingsViewModel,
        LogsViewModel logsViewModel)
    {
        this.navigationService = navigationService;
        this.apiClient = apiClient;
        this.realtimeClient = realtimeClient;
        this.dashboardViewModel = dashboardViewModel;
        this.projectsViewModel = projectsViewModel;
        this.taskCreationViewModel = taskCreationViewModel;
        this.workflowMonitorViewModel = workflowMonitorViewModel;
        this.diffReviewViewModel = diffReviewViewModel;
        this.settingsViewModel = settingsViewModel;
        this.logsViewModel = logsViewModel;

        currentView = dashboardViewModel;
        navigationService.Navigate(currentView);
        navigationService.Navigated += (_, viewModel) => CurrentView = viewModel;
    }

    [RelayCommand]
    public void ShowDashboard() => navigationService.Navigate(dashboardViewModel);

    [RelayCommand]
    public void ShowProjects() => navigationService.Navigate(projectsViewModel);

    [RelayCommand]
    public void ShowTaskCreation() => navigationService.Navigate(taskCreationViewModel);

    [RelayCommand]
    public void ShowWorkflowMonitor() => navigationService.Navigate(workflowMonitorViewModel);

    [RelayCommand]
    public void ShowDiffReview() => navigationService.Navigate(diffReviewViewModel);

    [RelayCommand]
    public void ShowSettings() => navigationService.Navigate(settingsViewModel);

    [RelayCommand]
    public void ShowLogs() => navigationService.Navigate(logsViewModel);

    [RelayCommand]
    public async Task InitializeAsync()
    {
        IsBusy = true;
        try
        {
            var apiHealthy = await apiClient.CheckHealthAsync(CancellationToken.None);
            await realtimeClient.StartAsync(CancellationToken.None);
            StatusText = $"API: {(apiHealthy ? "Connected" : "Unavailable")} | SignalR: {(realtimeClient.IsConnected ? "Connected" : "Disconnected")}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}

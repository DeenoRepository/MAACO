using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAACO.App.Services;
using System.Collections.ObjectModel;

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

    [ObservableProperty]
    private bool hasError;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public ObservableCollection<string> Notifications { get; } = [];

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
        realtimeClient.EventReceived += (_, message) => AddNotification(message);
        realtimeClient.ErrorOccurred += (_, message) =>
        {
            HasError = true;
            ErrorMessage = message;
            AddNotification(message);
        };
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
            HasError = false;
            ErrorMessage = string.Empty;
            StatusText = $"API: {(apiHealthy ? "Connected" : "Unavailable")} | SignalR: {(realtimeClient.IsConnected ? "Connected" : "Disconnected")}";
            AddNotification($"API status: {(apiHealthy ? "connected" : "unavailable")}");
            if (!apiHealthy)
            {
                HasError = true;
                ErrorMessage = "Backend API is unavailable.";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public void DismissError()
    {
        HasError = false;
        ErrorMessage = string.Empty;
    }

    [RelayCommand]
    public void DismissNotification(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        Notifications.Remove(message);
    }

    private void AddNotification(string message)
    {
        Notifications.Insert(0, $"{DateTime.Now:HH:mm:ss}  {message}");
        while (Notifications.Count > 5)
        {
            Notifications.RemoveAt(Notifications.Count - 1);
        }
    }
}

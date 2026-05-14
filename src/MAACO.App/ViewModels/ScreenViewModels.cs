namespace MAACO.App.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAACO.App.Services;
using System.Collections.ObjectModel;

public sealed class DashboardViewModel : BaseViewModel, IScreenViewModel
{
    public string Title => "Dashboard";
    public string Description => "Workflow health and quick actions.";
}

public sealed partial class WorkflowMonitorViewModel(IWorkflowsClient workflowsClient) : BaseViewModel, IScreenViewModel
{
    public string Title => "Workflow Monitor";
    public string Description => "Track workflow progress, retries, and status.";

    [ObservableProperty]
    private string workflowId = "Not started";

    [ObservableProperty]
    private string workflowStatus = "Idle";

    [ObservableProperty]
    private string retryCount = "0";

    [ObservableProperty]
    private string currentStep = "n/a";

    [ObservableProperty]
    private string timelineStatus = "No workflow selected.";

    public ObservableCollection<WorkflowStepItemViewModel> Steps { get; } = [];

    public void SetWorkflowSummary(Guid id, string status, int retries)
    {
        WorkflowId = id.ToString("D");
        WorkflowStatus = status;
        RetryCount = retries.ToString();
        _ = RefreshCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    public async Task RefreshAsync()
    {
        if (!Guid.TryParse(WorkflowId, out var workflowId))
        {
            TimelineStatus = "No workflow selected.";
            return;
        }

        var workflow = await workflowsClient.GetWorkflowAsync(workflowId, CancellationToken.None);
        var steps = await workflowsClient.GetWorkflowStepsAsync(workflowId, CancellationToken.None);

        Steps.Clear();
        foreach (var step in steps.OrderBy(x => x.Order))
        {
            var duration = step.UpdatedAt - step.CreatedAt;
            Steps.Add(new WorkflowStepItemViewModel(
                step.Name,
                step.Status,
                duration < TimeSpan.Zero ? "n/a" : duration.ToString(@"mm\:ss")));
        }

        if (workflow is not null)
        {
            WorkflowStatus = workflow.Status;
            RetryCount = workflow.RetryCount.ToString();
        }

        var active = steps
            .OrderBy(x => x.Order)
            .FirstOrDefault(x => string.Equals(x.Status, "Running", StringComparison.OrdinalIgnoreCase)
                              || string.Equals(x.Status, "Pending", StringComparison.OrdinalIgnoreCase));
        CurrentStep = active?.Name ?? steps.OrderByDescending(x => x.Order).FirstOrDefault()?.Name ?? "n/a";
        TimelineStatus = $"Loaded {steps.Count} step(s).";
    }
}

public sealed record WorkflowStepItemViewModel(
    string Name,
    string Status,
    string Duration);

public sealed class DiffReviewViewModel : BaseViewModel, IScreenViewModel
{
    public string Title => "Diff Review";
    public string Description => "Inspect proposed patch and commit message.";
}

public sealed class SettingsViewModel : BaseViewModel, IScreenViewModel
{
    public string Title => "Settings";
    public string Description => "Provider, timeout, and approval mode settings.";
}

public sealed class LogsViewModel : BaseViewModel, IScreenViewModel
{
    public string Title => "Logs";
    public string Description => "Realtime workflow logs and diagnostics.";
}

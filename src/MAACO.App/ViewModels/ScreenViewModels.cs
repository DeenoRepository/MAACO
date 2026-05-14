namespace MAACO.App.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;

public sealed class DashboardViewModel : BaseViewModel, IScreenViewModel
{
    public string Title => "Dashboard";
    public string Description => "Workflow health and quick actions.";
}

public sealed partial class WorkflowMonitorViewModel : BaseViewModel, IScreenViewModel
{
    public string Title => "Workflow Monitor";
    public string Description => "Track workflow progress, retries, and status.";

    [ObservableProperty]
    private string workflowId = "Not started";

    [ObservableProperty]
    private string workflowStatus = "Idle";

    [ObservableProperty]
    private string retryCount = "0";

    public void SetWorkflowSummary(Guid id, string status, int retries)
    {
        WorkflowId = id.ToString("D");
        WorkflowStatus = status;
        RetryCount = retries.ToString();
    }
}

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

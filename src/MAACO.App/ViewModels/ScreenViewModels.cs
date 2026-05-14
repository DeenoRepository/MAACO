namespace MAACO.App.ViewModels;

public sealed class DashboardViewModel : BaseViewModel, IScreenViewModel
{
    public string Title => "Dashboard";
    public string Description => "Workflow health and quick actions.";
}

public sealed class WorkflowMonitorViewModel : BaseViewModel, IScreenViewModel
{
    public string Title => "Workflow Monitor";
    public string Description => "Track workflow progress, retries, and status.";
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

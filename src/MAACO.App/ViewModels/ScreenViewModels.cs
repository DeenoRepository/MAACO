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

public sealed partial class WorkflowMonitorViewModel : BaseViewModel, IScreenViewModel
{
    private readonly IWorkflowsClient workflowsClient;
    private readonly IRealtimeClient realtimeClient;

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

    [ObservableProperty]
    private string realtimeStatus = "Disconnected";

    [ObservableProperty]
    private bool isRealtimeActive;

    [ObservableProperty]
    private bool hasErrors;

    [ObservableProperty]
    private string latestErrorMessage = "No errors detected.";

    public ObservableCollection<WorkflowStepItemViewModel> Steps { get; } = [];
    public ObservableCollection<WorkflowLogItemViewModel> Logs { get; } = [];
    public ObservableCollection<WorkflowLogItemViewModel> FilteredLogs { get; } = [];

    [ObservableProperty]
    private string severityFilter = "All";

    [ObservableProperty]
    private string agentFilter = string.Empty;

    [ObservableProperty]
    private string toolFilter = string.Empty;

    [ObservableProperty]
    private WorkflowLogItemViewModel? selectedLog;

    public IReadOnlyList<string> SeverityOptions { get; } = ["All", "Information", "Warning", "Error"];

    [ObservableProperty]
    private string activeAgent = "n/a";

    [ObservableProperty]
    private string activeTool = "n/a";

    [ObservableProperty]
    private string llmCallStatus = "unknown";

    [ObservableProperty]
    private string tokenEstimate = "0";

    partial void OnSeverityFilterChanged(string value) => ApplyLogFilters();
    partial void OnAgentFilterChanged(string value) => ApplyLogFilters();
    partial void OnToolFilterChanged(string value) => ApplyLogFilters();

    public WorkflowMonitorViewModel(
        IWorkflowsClient workflowsClient,
        IRealtimeClient realtimeClient)
    {
        this.workflowsClient = workflowsClient;
        this.realtimeClient = realtimeClient;
        realtimeClient.WorkflowEventReceived += OnWorkflowEventReceived;
    }

    public void SetWorkflowSummary(Guid id, string status, int retries)
    {
        WorkflowId = id.ToString("D");
        WorkflowStatus = status;
        RetryCount = retries.ToString();
        _ = realtimeClient.JoinWorkflowGroupAsync(id, CancellationToken.None);
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
        MarkCurrentStep();
    }

    [RelayCommand]
    public void CopySelectedLog()
    {
        if (SelectedLog is null)
        {
            TimelineStatus = "No log selected.";
            return;
        }

        TimelineStatus = $"Copied log line: [{SelectedLog.Severity}] {SelectedLog.Message}";
    }

    [RelayCommand]
    public async Task SaveLogsViewAsync()
    {
        var logsDir = Path.Combine(Environment.CurrentDirectory, ".maaco", "ui-logs");
        Directory.CreateDirectory(logsDir);
        var target = Path.Combine(logsDir, $"workflow-monitor-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.log");
        var lines = FilteredLogs.Select(x => $"{x.Timestamp:O} [{x.Severity}] [Tool:{x.Tool}] {x.Message}");
        await File.WriteAllLinesAsync(target, lines, CancellationToken.None);
        TimelineStatus = $"Saved logs view: {target}";
    }

    private void OnWorkflowEventReceived(object? sender, RealtimeEvent realtimeEvent)
    {
        if (!Guid.TryParse(WorkflowId, out _))
        {
            return;
        }

        IsRealtimeActive = true;
        RealtimeStatus = $"Live ({realtimeEvent.OccurredAt:HH:mm:ss})";

        Logs.Insert(0, new WorkflowLogItemViewModel(
            realtimeEvent.OccurredAt,
            realtimeEvent.Severity,
            realtimeEvent.Agent,
            realtimeEvent.Tool,
            realtimeEvent.Message));
        while (Logs.Count > 500)
        {
            Logs.RemoveAt(Logs.Count - 1);
        }

        ApplyLogFilters();
        UpdateAgentActivity(realtimeEvent);
        UpdateWorkflowProgress(realtimeEvent);
        UpdateErrorState(realtimeEvent);
    }

    private void ApplyLogFilters()
    {
        var filtered = Logs.Where(log =>
            (SeverityFilter == "All" || string.Equals(log.Severity, SeverityFilter, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrWhiteSpace(AgentFilter) || log.Agent.Contains(AgentFilter, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrWhiteSpace(ToolFilter) || log.Tool.Contains(ToolFilter, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        FilteredLogs.Clear();
        foreach (var log in filtered)
        {
            FilteredLogs.Add(log);
        }
    }

    private void UpdateAgentActivity(RealtimeEvent realtimeEvent)
    {
        if (!string.IsNullOrWhiteSpace(realtimeEvent.Agent) && realtimeEvent.Agent != "-")
        {
            ActiveAgent = realtimeEvent.Agent;
        }

        if (!string.IsNullOrWhiteSpace(realtimeEvent.Tool) && realtimeEvent.Tool != "-")
        {
            ActiveTool = realtimeEvent.Tool;
        }

        if (string.Equals(realtimeEvent.EventType, "ToolExecutionStarted", StringComparison.OrdinalIgnoreCase))
        {
            LlmCallStatus = "Tool running";
        }
        else if (string.Equals(realtimeEvent.EventType, "ToolExecutionCompleted", StringComparison.OrdinalIgnoreCase))
        {
            LlmCallStatus = "Tool completed";
        }
        else if (realtimeEvent.Message.Contains("Provider=", StringComparison.OrdinalIgnoreCase) ||
                 realtimeEvent.Message.Contains("Model=", StringComparison.OrdinalIgnoreCase))
        {
            LlmCallStatus = "LLM activity detected";
        }

        var estimate = Math.Max(1, realtimeEvent.Message.Length / 4);
        TokenEstimate = estimate.ToString();
    }

    private void UpdateWorkflowProgress(RealtimeEvent realtimeEvent)
    {
        if (string.Equals(realtimeEvent.EventType, "StepStarted", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(realtimeEvent.Message))
        {
            CurrentStep = realtimeEvent.Message;
            TimelineStatus = $"Running step: {CurrentStep}";
            MarkCurrentStep();
            return;
        }

        if (string.Equals(realtimeEvent.EventType, "StepCompleted", StringComparison.OrdinalIgnoreCase))
        {
            TimelineStatus = $"Completed step: {CurrentStep}";
            return;
        }

        if (string.Equals(realtimeEvent.EventType, "WorkflowCompleted", StringComparison.OrdinalIgnoreCase))
        {
            WorkflowStatus = "Completed";
            TimelineStatus = "Workflow completed.";
            return;
        }

        if (string.Equals(realtimeEvent.EventType, "WorkflowFailed", StringComparison.OrdinalIgnoreCase))
        {
            WorkflowStatus = "Failed";
            TimelineStatus = "Workflow failed.";
        }
    }

    private void UpdateErrorState(RealtimeEvent realtimeEvent)
    {
        if (string.Equals(realtimeEvent.Severity, "Error", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(realtimeEvent.EventType, "StepFailed", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(realtimeEvent.EventType, "WorkflowFailed", StringComparison.OrdinalIgnoreCase))
        {
            HasErrors = true;
            LatestErrorMessage = realtimeEvent.Message;
        }
    }

    private void MarkCurrentStep()
    {
        foreach (var step in Steps)
        {
            step.IsCurrent = string.Equals(step.Name, CurrentStep, StringComparison.OrdinalIgnoreCase);
        }
    }
}

public sealed partial class WorkflowStepItemViewModel : ObservableObject
{
    public WorkflowStepItemViewModel(string name, string status, string duration)
    {
        Name = name;
        Status = status;
        Duration = duration;
    }

    public string Name { get; }
    public string Status { get; }
    public string Duration { get; }

    [ObservableProperty]
    private bool isCurrent;

    [ObservableProperty]
    private string marker = string.Empty;

    partial void OnIsCurrentChanged(bool value)
    {
        Marker = value ? ">>" : string.Empty;
    }
}

public sealed record WorkflowLogItemViewModel(
    DateTimeOffset Timestamp,
    string Severity,
    string Agent,
    string Tool,
    string Message);

public sealed partial class DiffReviewViewModel : BaseViewModel, IScreenViewModel
{
    private readonly ITasksClient tasksClient;

    public string Title => "Diff Review";
    public string Description => "Inspect proposed patch and commit message.";

    [ObservableProperty]
    private string taskId = string.Empty;

    [ObservableProperty]
    private string diffStatus = "No diff loaded.";

    [ObservableProperty]
    private string diffText = string.Empty;

    public ObservableCollection<string> ChangedFiles { get; } = [];

    public DiffReviewViewModel(ITasksClient tasksClient)
    {
        this.tasksClient = tasksClient;
    }

    [RelayCommand]
    public async Task LoadDiffAsync()
    {
        ChangedFiles.Clear();

        if (!Guid.TryParse(TaskId, out var parsedTaskId))
        {
            DiffStatus = "Enter valid task id.";
            DiffText = string.Empty;
            return;
        }

        var diff = await tasksClient.GetTaskDiffAsync(parsedTaskId, CancellationToken.None);
        if (diff is null)
        {
            DiffStatus = "Failed to load diff.";
            DiffText = string.Empty;
            return;
        }

        DiffStatus = $"Diff status: {diff.Status}";
        DiffText = diff.Diff;

        foreach (var file in ExtractChangedFiles(diff.Diff))
        {
            ChangedFiles.Add(file);
        }

        if (ChangedFiles.Count == 0)
        {
            ChangedFiles.Add("No changed files parsed from diff.");
        }
    }

    private static IReadOnlyList<string> ExtractChangedFiles(string diffText)
    {
        var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var lines = diffText.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            if (line.StartsWith("+++ b/", StringComparison.Ordinal))
            {
                files.Add(line["+++ b/".Length..]);
            }
            else if (line.StartsWith("--- a/", StringComparison.Ordinal))
            {
                files.Add(line["--- a/".Length..]);
            }
        }

        return files.OrderBy(x => x).ToList();
    }
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

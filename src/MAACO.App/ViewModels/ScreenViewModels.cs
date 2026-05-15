namespace MAACO.App.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAACO.App.Services;
using MAACO.App.Services.Models;
using System.Collections.ObjectModel;
using Avalonia.Media;

public sealed partial class DashboardViewModel : BaseViewModel, IScreenViewModel
{
    public string Title => "Dashboard";
    public string Description => "Workflow health and quick actions.";

    [ObservableProperty]
    private string lastRefresh = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss");

    [ObservableProperty]
    private string apiHealthSummary = "Backend connectivity is verified on application startup.";

    [ObservableProperty]
    private string workflowSummary = "Use Task Creation to launch a workflow, then monitor execution in Workflow Monitor.";

    [ObservableProperty]
    private string approvalSummary = "Diff Review supports approve/reject with editable commit message.";

    [RelayCommand]
    private void RefreshOverview()
    {
        LastRefresh = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
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

    [ObservableProperty]
    private string generatedCommitMessage = string.Empty;

    [ObservableProperty]
    private string editableCommitMessage = string.Empty;

    [ObservableProperty]
    private string approvalStatusMessage = "No approval action yet.";

    [ObservableProperty]
    private string rejectReason = string.Empty;

    public ObservableCollection<string> ChangedFiles { get; } = [];
    public ObservableCollection<DiffLineViewModel> DiffLines { get; } = [];

    public DiffReviewViewModel(ITasksClient tasksClient)
    {
        this.tasksClient = tasksClient;
    }

    [RelayCommand]
    public async Task LoadDiffAsync()
    {
        ChangedFiles.Clear();
        DiffLines.Clear();

        if (!Guid.TryParse(TaskId, out var parsedTaskId))
        {
            DiffStatus = "Enter valid task id.";
            DiffText = string.Empty;
            GeneratedCommitMessage = string.Empty;
            EditableCommitMessage = string.Empty;
            return;
        }

        var task = await tasksClient.GetTaskByIdAsync(parsedTaskId, CancellationToken.None);
        var diff = await tasksClient.GetTaskDiffAsync(parsedTaskId, CancellationToken.None);
        if (diff is null)
        {
            DiffStatus = "Failed to load diff.";
            DiffText = string.Empty;
            GeneratedCommitMessage = string.Empty;
            EditableCommitMessage = string.Empty;
            return;
        }

        DiffStatus = $"Diff status: {diff.Status}";
        DiffText = diff.Diff;

        foreach (var file in ExtractChangedFiles(diff.Diff))
        {
            ChangedFiles.Add(file);
        }

        foreach (var line in BuildDiffLines(diff.Diff))
        {
            DiffLines.Add(line);
        }

        if (ChangedFiles.Count == 0)
        {
            ChangedFiles.Add("No changed files parsed from diff.");
        }

        GeneratedCommitMessage = BuildGeneratedCommitMessage(task, ChangedFiles);
        EditableCommitMessage = GeneratedCommitMessage;
    }

    [RelayCommand]
    public void ResetCommitMessage()
    {
        EditableCommitMessage = GeneratedCommitMessage;
    }

    [RelayCommand]
    public async Task ApproveAsync()
    {
        if (!Guid.TryParse(TaskId, out var parsedTaskId))
        {
            ApprovalStatusMessage = "Enter valid task id before approve.";
            return;
        }

        var response = await tasksClient.CommitTaskAsync(parsedTaskId, CancellationToken.None);
        if (response is null)
        {
            ApprovalStatusMessage = "Approve request failed.";
            return;
        }

        ApprovalStatusMessage = $"Approve: {response.Status} - {response.Message}";
    }

    [RelayCommand]
    public async Task RejectAsync()
    {
        if (!Guid.TryParse(TaskId, out var parsedTaskId))
        {
            ApprovalStatusMessage = "Enter valid task id before reject.";
            return;
        }

        var response = await tasksClient.RollbackTaskAsync(parsedTaskId, RejectReason, CancellationToken.None);
        if (response is null)
        {
            ApprovalStatusMessage = "Reject request failed.";
            return;
        }

        ApprovalStatusMessage = $"Reject: {response.Status} - {response.Message}";
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

    private static IReadOnlyList<DiffLineViewModel> BuildDiffLines(string diffText)
    {
        var lines = diffText.Split(['\r', '\n'], StringSplitOptions.None);
        var result = new List<DiffLineViewModel>(lines.Length);
        foreach (var line in lines)
        {
            if (line.StartsWith('+') && !line.StartsWith("+++", StringComparison.Ordinal))
            {
                result.Add(new DiffLineViewModel(line, "Added", new SolidColorBrush(Color.Parse("#73E2A7"))));
            }
            else if (line.StartsWith('-') && !line.StartsWith("---", StringComparison.Ordinal))
            {
                result.Add(new DiffLineViewModel(line, "Removed", new SolidColorBrush(Color.Parse("#FF8A8A"))));
            }
            else
            {
                result.Add(new DiffLineViewModel(line, "Context", new SolidColorBrush(Color.Parse("#D8DEE9"))));
            }
        }

        return result;
    }

    private static string BuildGeneratedCommitMessage(TaskDto? task, IReadOnlyCollection<string> changedFiles)
    {
        var normalizedTitle = string.IsNullOrWhiteSpace(task?.Title)
            ? "update project changes"
            : task!.Title.Trim().ToLowerInvariant();

        var summary = changedFiles.Count == 0
            ? "no files listed"
            : $"{changedFiles.Count} file(s) changed";

        return $"feat(maaco): {normalizedTitle}{Environment.NewLine}{Environment.NewLine}- {summary}";
    }
}

public sealed record DiffLineViewModel(
    string Text,
    string Kind,
    IBrush Foreground);

public sealed partial class SettingsViewModel : BaseViewModel, IScreenViewModel
{
    public string Title => "Settings";
    public string Description => "Provider, timeout, and approval mode settings.";

    [ObservableProperty]
    private string selectedProvider = "OpenAI-compatible";

    [ObservableProperty]
    private string baseUrl = "http://localhost:11434";

    [ObservableProperty]
    private string model = "llama3.1";

    [ObservableProperty]
    private string timeoutSeconds = "120";

    [ObservableProperty]
    private bool approvalRequired = true;

    [ObservableProperty]
    private string settingsStatus = "Settings are ready to be adjusted.";

    public IReadOnlyList<string> Providers { get; } = ["OpenAI-compatible", "Ollama", "Fake"];

    [RelayCommand]
    private void TestConnection()
    {
        SettingsStatus = $"Connection test simulated for {SelectedProvider} at {DateTimeOffset.Now:HH:mm:ss}.";
    }

    [RelayCommand]
    private void SaveSettings()
    {
        SettingsStatus = $"Settings saved at {DateTimeOffset.Now:HH:mm:ss}.";
    }
}

public sealed partial class LogsViewModel : BaseViewModel, IScreenViewModel
{
    public string Title => "Logs";
    public string Description => "Realtime workflow logs and diagnostics.";

    [ObservableProperty]
    private string logsStatus = "Load logs to inspect generated diagnostic files.";

    [ObservableProperty]
    private string selectedLogPath = "No file selected.";

    [ObservableProperty]
    private string selectedLogPreview = "Select a log file to preview content.";

    public ObservableCollection<string> LogFiles { get; } = [];

    public LogsViewModel()
    {
        _ = RefreshLogsCommand.ExecuteAsync(null);
    }

    partial void OnSelectedLogPathChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value == "No file selected.")
        {
            return;
        }

        _ = OpenLogCommand.ExecuteAsync(value);
    }

    [RelayCommand]
    private async Task RefreshLogsAsync()
    {
        var logsRoot = Path.Combine(Environment.CurrentDirectory, ".maaco", "ui-logs");
        Directory.CreateDirectory(logsRoot);

        var files = Directory.EnumerateFiles(logsRoot, "*.log")
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .Take(50)
            .ToList();

        LogFiles.Clear();
        foreach (var file in files)
        {
            LogFiles.Add(file);
        }

        LogsStatus = $"Loaded {LogFiles.Count} file(s) from {logsRoot}.";
        if (LogFiles.Count == 0)
        {
            SelectedLogPath = "No file selected.";
            SelectedLogPreview = "No log files found yet. Run workflow monitor save action first.";
        }
        else
        {
            await OpenLogAsync(LogFiles[0], CancellationToken.None);
        }
    }

    [RelayCommand]
    private async Task OpenLogAsync(string? filePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            LogsStatus = "Selected log file not found.";
            return;
        }

        SelectedLogPath = filePath;
        var lines = await File.ReadAllLinesAsync(filePath, cancellationToken);
        SelectedLogPreview = string.Join(Environment.NewLine, lines.Take(120));
        LogsStatus = $"Preview loaded: {Path.GetFileName(filePath)}";
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAACO.App.Services;
using MAACO.App.Services.Models;
using System.Collections.ObjectModel;

namespace MAACO.App.ViewModels;

public sealed partial class TaskCreationViewModel(
    IProjectsClient projectsClient,
    ITasksClient tasksClient,
    IWorkflowsClient workflowsClient,
    INavigationService navigationService,
    WorkflowMonitorViewModel workflowMonitorViewModel) : BaseViewModel, IScreenViewModel
{
    public string Title => "Task Creation";
    public string Description => "Create autonomous SDLC tasks with constraints.";

    public ObservableCollection<ProjectDto> Projects { get; } = [];
    public IReadOnlyList<string> ApprovalModes { get; } = ["Manual", "Auto"];

    [ObservableProperty]
    private ProjectDto? selectedProject;

    [ObservableProperty]
    private string taskTitle = string.Empty;

    [ObservableProperty]
    private string taskDescription = string.Empty;

    [ObservableProperty]
    private string constraints = string.Empty;

    [ObservableProperty]
    private string selectedApprovalMode = "Manual";

    [ObservableProperty]
    private string status = "Ready";

    [ObservableProperty]
    private bool isBusy;

    [RelayCommand]
    public async Task LoadProjectsAsync()
    {
        IsBusy = true;
        try
        {
            Projects.Clear();
            var projects = await projectsClient.ListProjectsAsync(CancellationToken.None);
            foreach (var project in projects.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase))
            {
                Projects.Add(project);
            }

            SelectedProject = Projects.FirstOrDefault();
            Status = Projects.Count == 0
                ? "No projects found. Add a project first."
                : $"Loaded {Projects.Count} project(s).";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task CreateTaskAsync()
    {
        if (SelectedProject is null)
        {
            Status = "Select a project.";
            return;
        }

        if (string.IsNullOrWhiteSpace(TaskTitle))
        {
            Status = "Task title is required.";
            return;
        }

        IsBusy = true;
        try
        {
            var description = string.IsNullOrWhiteSpace(TaskDescription)
                ? null
                : $"{TaskDescription.Trim()}\n\nConstraints: {Constraints.Trim()}\nApprovalMode: {SelectedApprovalMode}";

            var task = await tasksClient.CreateTaskAsync(
                SelectedProject.Id,
                TaskTitle.Trim(),
                description,
                CancellationToken.None);

            if (task is null)
            {
                Status = "Failed to create task.";
                return;
            }

            Status = $"Task created: {task.Id:D}. Starting workflow...";
            var workflowStart = await workflowsClient.StartWorkflowAsync(
                task.Id,
                trigger: "ui-task-start",
                CancellationToken.None);
            if (workflowStart is null)
            {
                Status = $"Task created: {task.Id:D}, but workflow start failed.";
                return;
            }

            workflowMonitorViewModel.SetWorkflowSummary(
                workflowStart.WorkflowId,
                workflowStart.Status,
                retries: 0);
            Status = $"Workflow queued: {workflowStart.WorkflowId:D}. Opening workflow monitor.";
            navigationService.Navigate(workflowMonitorViewModel);
        }
        finally
        {
            IsBusy = false;
        }
    }
}

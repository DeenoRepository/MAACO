using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAACO.App.Services;

namespace MAACO.App.ViewModels;

public sealed partial class ProjectsViewModel(IProjectsClient projectsClient) : BaseViewModel, IScreenViewModel
{
    public string Title => "Projects";
    public string Description => "Repository onboarding and project scan.";

    [ObservableProperty]
    private string selectedFolderPath = string.Empty;

    [ObservableProperty]
    private string projectName = string.Empty;

    [ObservableProperty]
    private string detectedStack = "unknown";

    [ObservableProperty]
    private string buildCommand = string.Empty;

    [ObservableProperty]
    private string testCommand = string.Empty;

    [ObservableProperty]
    private string projectFilesSummary = "No scan results yet.";

    [ObservableProperty]
    private string status = "Ready";

    [ObservableProperty]
    private bool isBusy;

    [RelayCommand]
    public void UseCurrentFolder()
    {
        SelectedFolderPath = Environment.CurrentDirectory;
        if (string.IsNullOrWhiteSpace(ProjectName))
        {
            ProjectName = Path.GetFileName(SelectedFolderPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        }
    }

    [RelayCommand]
    public async Task AddAndScanProjectAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedFolderPath))
        {
            Status = "Folder path is required.";
            return;
        }

        var name = string.IsNullOrWhiteSpace(ProjectName)
            ? Path.GetFileName(SelectedFolderPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
            : ProjectName.Trim();

        IsBusy = true;
        try
        {
            Status = "Creating project...";
            var createResult = await projectsClient.CreateProjectAsync(name, SelectedFolderPath.Trim(), CancellationToken.None);
            if (createResult.Project is null)
            {
                var reason = string.IsNullOrWhiteSpace(createResult.ErrorMessage)
                    ? "Unknown API error."
                    : createResult.ErrorMessage;
                Status = $"Failed to create project: {reason}";
                return;
            }
            var project = createResult.Project;

            Status = "Running project scan...";
            var scan = await projectsClient.ScanProjectAsync(project.Id, CancellationToken.None);
            if (scan is null)
            {
                Status = "Project created, but scan failed.";
                return;
            }

            DetectedStack = scan.PrimaryStack;
            BuildCommand = scan.BuildCommand;
            TestCommand = scan.TestCommand;

            var keyFiles = scan.SolutionFiles
                .Concat(scan.ProjectFiles)
                .Concat(scan.PackageManifests)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(8)
                .ToArray();

            ProjectFilesSummary = keyFiles.Length == 0
                ? $"Scanned files: {scan.ScannedFiles}. No key files detected."
                : $"Scanned files: {scan.ScannedFiles}. Key files: {string.Join(", ", keyFiles)}";
            Status = "Project added and scanned.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}

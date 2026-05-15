using Avalonia.Controls;
using MAACO.App.ViewModels;

namespace MAACO.App.Views;

public partial class ProjectsView : UserControl
{
    public ProjectsView()
    {
        InitializeComponent();
    }

    private async void BrowseFolder_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.StorageProvider is null || !topLevel.StorageProvider.CanPickFolder)
        {
            return;
        }

        var picked = await topLevel.StorageProvider.OpenFolderPickerAsync(
            new Avalonia.Platform.Storage.FolderPickerOpenOptions
            {
                Title = "Select repository folder",
                AllowMultiple = false
            });

        var folder = picked.FirstOrDefault();
        if (folder is null || DataContext is not ProjectsViewModel viewModel)
        {
            return;
        }

        viewModel.SelectedFolderPath = folder.Path.LocalPath;
        if (string.IsNullOrWhiteSpace(viewModel.ProjectName))
        {
            viewModel.ProjectName = Path.GetFileName(folder.Path.LocalPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        }
    }
}

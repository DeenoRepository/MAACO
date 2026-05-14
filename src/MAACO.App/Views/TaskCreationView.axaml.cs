using Avalonia;
using Avalonia.Controls;
using MAACO.App.ViewModels;

namespace MAACO.App.Views;

public partial class TaskCreationView : UserControl
{
    public TaskCreationView()
    {
        InitializeComponent();
    }

    protected override async void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (DataContext is TaskCreationViewModel viewModel)
        {
            await viewModel.LoadProjectsCommand.ExecuteAsync(null);
        }
    }
}

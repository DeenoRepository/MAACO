using MAACO.App.ViewModels;

namespace MAACO.App.Services;

public sealed class NavigationService : INavigationService
{
    private BaseViewModel currentView = new DashboardViewModel();

    public BaseViewModel CurrentView => currentView;

    public event EventHandler<BaseViewModel>? Navigated;

    public void Navigate(BaseViewModel viewModel)
    {
        currentView = viewModel;
        Navigated?.Invoke(this, viewModel);
    }
}

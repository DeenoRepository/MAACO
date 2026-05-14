using MAACO.App.ViewModels;

namespace MAACO.App.Services;

public interface INavigationService
{
    BaseViewModel CurrentView { get; }
    event EventHandler<BaseViewModel>? Navigated;
    void Navigate(BaseViewModel viewModel);
}

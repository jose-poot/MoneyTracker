using MoneyTracker.Presentation.ViewModels;

namespace MoneyTracker.Presentation.Services.Interfaces;

public interface INavigationService
{
    Task NavigateToAsync<TViewModel>() where TViewModel : BaseViewModel;
    Task NavigateToAsync<TViewModel>(object parameters) where TViewModel : BaseViewModel;
    Task NavigateToAsync(string route, object? parameters = null);
    Task NavigateBackAsync();
    Task NavigateToRootAsync();
    bool CanNavigateBack { get; }
}
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.Infrastructure.Api;

namespace MoneyTracker.Presentation.Binding;

public abstract class ActivityBase<TViewModel> : Activity where TViewModel : ObservableObject
{
    protected TViewModel ViewModel { get; private set; } = default!;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        ViewModel = AppServices.ServiceProvider.GetRequiredService<TViewModel>();
    }
}
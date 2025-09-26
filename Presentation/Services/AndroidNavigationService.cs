using Android.Content;
using Android.OS;
using AndroidX.AppCompat.App;
using AndroidX.Fragment.App;
using MoneyTracker.Application.DTOs;
using MoneyTracker.Presentation.Activities;
using MoneyTracker.Presentation.Fragments;
using MoneyTracker.Presentation.Navigation;
using MoneyTracker.Presentation.Services.Interfaces;
using MoneyTracker.Presentation.ViewModels;
using System;

namespace MoneyTracker.Presentation.Services;

public class AndroidNavigationService : INavigationService
{
    private readonly Func<Activity> _currentActivityProvider;

    public AndroidNavigationService(Func<Activity> currentActivityProvider)
    {
        _currentActivityProvider = currentActivityProvider;
    }

    public bool CanNavigateBack
    {
        get
        {
            var activity = _currentActivityProvider();
            return activity is AppCompatActivity appActivity &&
                   appActivity.SupportFragmentManager.BackStackEntryCount > 0;
        }
    }

    public Task NavigateToAsync<TViewModel>() where TViewModel : BaseViewModel
    {
        var route = GetRouteFromViewModelType(typeof(TViewModel));
        return NavigateToAsync(route);
    }

    public Task NavigateToAsync<TViewModel>(object parameters) where TViewModel : BaseViewModel
    {
        var route = GetRouteFromViewModelType(typeof(TViewModel));
        return NavigateToAsync(route, parameters);
    }

    public Task NavigateToAsync(string route, object? parameters = null)
    {
        return route switch
        {
            "AddTransaction" => NavigateToFragment(new AddTransactionFragment(), parameters, "AddTransaction"),
            "EditTransaction" => NavigateToEditTransaction(parameters),
            "Settings" => NavigateToActivity<SettingsActivity>(parameters),
            _ => throw new ArgumentException($"Unknown route: {route}")
        };
    }

    private Task NavigateToEditTransaction(object? parameters)
    {
        if (parameters is TransactionDto transaction)
        {
            var fragment = AddTransactionFragment.NewInstanceForEdit(transaction);
            return NavigateToFragment(fragment, null, "EditTransaction");
        }

        throw new ArgumentException("EditTransaction navigation requires a TransactionDto parameter.", nameof(parameters));
    }

    public Task NavigateBackAsync()
    {
        var activity = _currentActivityProvider();

        if (activity is AppCompatActivity appActivity)
        {
            if (appActivity.SupportFragmentManager.BackStackEntryCount > 0)
            {
                appActivity.SupportFragmentManager.PopBackStack();
            }
            else
            {
                activity.Finish();
            }
        }

        return Task.CompletedTask;
    }

    public Task NavigateToRootAsync()
    {
        var activity = _currentActivityProvider();

        if (activity is AppCompatActivity appActivity)
        {
            // Clear back stack
            appActivity.SupportFragmentManager.PopBackStack(null,
                FragmentManager.PopBackStackInclusive);
        }

        return Task.CompletedTask;
    }

    private Task NavigateToFragment<TFragment>(object? parameters, string? backStackTag = null) where TFragment : Fragment, new()
    {
        var fragment = new TFragment();
        var tag = backStackTag ?? typeof(TFragment).Name;
        return NavigateToFragment(fragment, parameters, tag);
    }

    private Task NavigateToFragment(Fragment fragment, object? parameters, string? backStackTag)
    {
        var activity = _currentActivityProvider();

        if (activity is AppCompatActivity appActivity)
        {
            if (parameters != null)
            {
                var args = CreateFragmentArguments(parameters);
                if (fragment.Arguments == null)
                {
                    fragment.Arguments = args;
                }
                else
                {
                    fragment.Arguments.PutAll(args);
                }
            }

            var transaction = appActivity.SupportFragmentManager.BeginTransaction();
            transaction.Replace(Resource.Id.fragment_container, fragment);

            if (!string.IsNullOrEmpty(backStackTag))
            {
                transaction.AddToBackStack(backStackTag);
            }

            transaction.CommitAllowingStateLoss();
        }

        return Task.CompletedTask;
    }

    private Task NavigateToActivity<TActivity>(object? parameters) where TActivity : Activity
    {
        var currentActivity = _currentActivityProvider();
        var intent = new Intent(currentActivity, typeof(TActivity));

        if (parameters != null)
        {
            // Serialize parameters if needed
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(parameters);
            intent.PutExtra("parameters", json);
        }

        currentActivity.StartActivity(intent);
        return Task.CompletedTask;
    }

    private string GetRouteFromViewModelType(Type viewModelType)
    {
        return viewModelType.Name switch
        {
            nameof(AddTransactionViewModel) => "AddTransaction",
            nameof(SettingsViewModel) => "Settings",
            _ => viewModelType.Name.Replace("ViewModel", "")
        };
    }

    private Bundle CreateFragmentArguments(object parameters)
    {
        var bundle = new Bundle();
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(parameters);
        bundle.PutString(NavigationParameterKeys.FragmentParameters, json);
        return bundle;
    }
}

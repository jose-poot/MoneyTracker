using Android;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using Microsoft.Extensions.DependencyInjection;
using Java.Lang;

using CommunityToolkit.Mvvm.Messaging;
using MoneyTracker.Application.DTOs;
using MoneyTracker.Presentation.Fragments;
using MoneyTracker.Presentation.Messages;   // <-- tus mensajes tipados
using MoneyTracker.Presentation.ViewModels;

namespace MoneyTracker.Presentation.Activities
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            if (savedInstanceState == null)
            {
                LoadInitialFragment();
            }

            SubscribeToNavigationEvents();
        }

        protected override void OnDestroy()
        {
            // ✅ Con WeakReferenceMessenger: limpiar todas las suscripciones de este recipient
            WeakReferenceMessenger.Default.UnregisterAll(this);
            base.OnDestroy();
        }

        private void LoadInitialFragment()
        {
            if (IsActivityValid())
            {
                var fragment = new TransactionListFragment();
                SupportFragmentManager
                    .BeginTransaction()
                    .Replace(Resource.Id.fragment_container, fragment)
                    .Commit();
            }
        }

        private void SubscribeToNavigationEvents()
        {
            // NavigateToAddTransaction
            WeakReferenceMessenger.Default.Register<NavigateToAddTransactionMessage>(this, (recipient, msg) =>
            {
                if (IsActivityValid())
                {
                    var fragment = new AddTransactionFragment();
                    NavigateToFragment(fragment, "AddTransaction");
                }
            });

            // NavigateToEditTransaction (lleva TransactionDto en msg.Value)
            WeakReferenceMessenger.Default.Register<NavigateToEditTransactionMessage>(this, (recipient, msg) =>
            {
                if (IsActivityValid())
                {
                    var fragment = AddTransactionFragment.NewInstanceForEdit(msg.Value);
                    NavigateToFragment(fragment, "EditTransaction");
                }
            });

            // NavigateBack
            WeakReferenceMessenger.Default.Register<NavigateBackMessage>(this, (recipient, msg) =>
            {
                if (IsActivityValid())
                {
                    if (SupportFragmentManager.BackStackEntryCount > 0)
                    {
                        SupportFragmentManager.PopBackStack();
                    }
                    else
                    {
                        Finish();
                    }
                }
            });

            // ShowMessage (Toast/Snackbar)
            WeakReferenceMessenger.Default.Register<ShowMessageMessage>(this, (recipient, msg) =>
            {
                if (IsActivityValid())
                {
                    ShowToast(msg.Value);
                }
            });

            // ShowError (Dialog)
            WeakReferenceMessenger.Default.Register<ShowErrorMessage>(this, (recipient, msg) =>
            {
                if (IsActivityValid())
                {
                    ShowErrorDialog(msg.Value);
                }
            });
        }

        private bool IsActivityValid()
        {
            return !IsFinishing && !IsDestroyed && SupportFragmentManager != null && !SupportFragmentManager.IsDestroyed;
        }

        private void NavigateToFragment(AndroidX.Fragment.App.Fragment fragment, string tag)
        {
            try
            {
                if (IsActivityValid())
                {
                    var transaction = SupportFragmentManager.BeginTransaction();
                    transaction.Replace(Resource.Id.fragment_container, fragment);
                    transaction.AddToBackStack(tag);
                    transaction.CommitAllowingStateLoss(); // más seguro si el estado ya se guardó
                }
            }
            catch (Java.Lang.IllegalStateException ex)
            {
                System.Diagnostics.Debug.WriteLine($"NavigateToFragment error: {ex.Message}");
            }
        }

        private void ShowToast(string message)
        {
            try
            {
                RunOnUiThread(() =>
                {
                    if (IsActivityValid())
                    {
                        Toast.MakeText(this, message, ToastLength.Short)?.Show();
                    }
                });
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ShowToast error: {ex.Message}");
            }
        }

        private void ShowErrorDialog(string error)
        {
            try
            {
                RunOnUiThread(() =>
                {
                    if (IsActivityValid())
                    {
                        new AndroidX.AppCompat.App.AlertDialog.Builder(this)
                            .SetTitle("Error")
                            .SetMessage(error)
                            .SetPositiveButton("OK", (sender, e) => { })
                            .Show();
                    }
                });
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ShowErrorDialog error: {ex.Message}");
            }
        }

        public override bool OnCreateOptionsMenu(IMenu? menu)
        {
            base.OnCreateOptionsMenu(menu);
            // Forzamos overload con ICharSequence para evitar cast a resourceId (int)
            menu?.Add(0, 1001, 0, new Java.Lang.String("Settings"));
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem? item)
        {
            if (item?.ItemId == 1001)
            {
                var nav = MoneyTracker.Presentation.Binding.AppServices.ServiceProvider
                    .GetRequiredService<MoneyTracker.Presentation.Navigation.INavigator>();
                nav.GoTo<MoneyTracker.Presentation.Activities.SettingsActivity>(this);
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }
    }
}

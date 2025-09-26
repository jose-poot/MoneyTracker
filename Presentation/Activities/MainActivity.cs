using Android.App;
using Android.OS;
using Android.Views;
using Java.Lang;
using MoneyTracker.Presentation.Base;
using MoneyTracker.Presentation.Fragments;
using MoneyTracker.Presentation.ViewModels;


namespace MoneyTracker.Presentation.Activities
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : ActivityBase<TransactionListViewModel>
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            if (savedInstanceState == null)
            {
                LoadInitialFragment();
            }
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

        private bool IsActivityValid()
        {
            return !IsFinishing && !IsDestroyed && SupportFragmentManager != null && !SupportFragmentManager.IsDestroyed;
        }

        public override bool OnCreateOptionsMenu(IMenu? menu)
        {
            base.OnCreateOptionsMenu(menu);
            // Force the overload with ICharSequence to avoid casting to a resource ID (int)
            menu?.Add(0, 1001, 0, new Java.Lang.String("Settings"));
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == 1001)
            {
                if (NavigationService != null)
                {
                    _ = NavigationService.NavigateToAsync("Settings");
                    return true;
                }
            }
            return base.OnOptionsItemSelected(item);
        }
    }
}

using Android;
using Android.Views;
using AndroidX.AppCompat.App;
using Microsoft.Extensions.DependencyInjection;
using Java.Lang;

using MoneyTracker.Presentation.Fragments;

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

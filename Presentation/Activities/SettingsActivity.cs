using Android.App;
using Android.OS;
using Android.Widget;
using MoneyTracker.Presentation.Base;
using MoneyTracker.Presentation.ViewModels;
using System;

namespace MoneyTracker.Presentation.Activities
{
    [Activity(Label = "Settings")]
    public sealed class SettingsActivity : ActivityBase<SettingsViewModel>
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_settings);

            var txtUser = FindViewById<EditText>(Resource.Id.txtUserName)
                ?? throw new InvalidOperationException("The user name field was not found in the layout.");
            var btnSave = FindViewById<Button>(Resource.Id.btnSave)
                ?? throw new InvalidOperationException("The save button was not found in the layout.");

            var set = CreateDataBindingSet();

            set.Bind(txtUser).To<string>(vm => ((SettingsViewModel)vm).UserName).TwoWay();
            set.Bind(btnSave).To(vm => ((SettingsViewModel)vm).SaveCommand);

            set.Apply();
            ApplyDataBindings();

            _ = ViewModel.LoadCommand.ExecuteAsync(null);
        }
    }
}

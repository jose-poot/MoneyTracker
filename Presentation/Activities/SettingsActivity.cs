using Android;
using Android.App;
using Android.OS;
using Android.Widget;
using MoneyTracker.Presentation.Base;
using MoneyTracker.Presentation.Binding;
using MoneyTracker.Presentation.ViewModels;

namespace MoneyTracker.Presentation.Activities
{
    [Activity(Label = "Settings")]
    public sealed class SettingsActivity : ActivityBase<SettingsViewModel>
    {
        protected override async void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_settings);

        //    var txtUser = FindViewById<EditText>(Resource.Id.txtUserName)!;
        //    var btnSave = FindViewById<Button>(Resource.Id.btnSave)!;

        //    // ✅ Usar el comando generado, no el método directamente
        //    await ViewModel.LoadAsyncCommand.ExecuteAsync(null);

        //    var set = new BindingSet<SettingsViewModel>(ViewModel);
        //    set.BindText(txtUser, vm => vm.UserName, (vm, v) => vm.UserName = v ?? string.Empty);

        //    // ✅ Usar el comando correcto que se genera automáticamente
        //    set.BindClick(btnSave, ViewModel.SaveAsyncCommand);
        }
    }
}
using MoneyTracker.Presentation.Base;
using MoneyTracker.Presentation.ViewModels;
using Android.Widget;

namespace MoneyTracker.Presentation.Activities
{
    [Activity(Label = "Agregar Transacción")]
    public class AddTransactionActivity : ActivityBase<AddTransactionViewModel>
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.fragment_add_transaction);

            var editDescription = FindViewById<EditText>(Resource.Id.edit_description)!;
            var editAmount = FindViewById<EditText>(Resource.Id.edit_amount)!;
            var editNotes = FindViewById<EditText>(Resource.Id.edit_notes)!;
            var editLocation = FindViewById<EditText>(Resource.Id.edit_location)!;
            var buttonSave = FindViewById<Button>(Resource.Id.button_save)!;
            var buttonCancel = FindViewById<Button>(Resource.Id.button_cancel)!;

            var set = CreateDataBindingSet();

            // ✅ CORRECCIÓN: Cast explícito del ViewModel
            set.Bind(editDescription).To<string>(vm => ((AddTransactionViewModel)vm).Description).TwoWay();
            set.Bind(editAmount).To<decimal>(vm => ((AddTransactionViewModel)vm).Amount).TwoWay();
            set.Bind(editNotes).To<string>(vm => ((AddTransactionViewModel)vm).Notes).TwoWay();
            set.Bind(editLocation).To<string>(vm => ((AddTransactionViewModel)vm).Location).TwoWay();

            // Commands
            set.Bind(buttonSave).To(vm => ((AddTransactionViewModel)vm).SaveTransactionCommand);
            set.Bind(buttonCancel).To(vm => ((AddTransactionViewModel)vm).CancelCommand);

            set.Apply();
            ApplyDataBindings();
        }
    }
}
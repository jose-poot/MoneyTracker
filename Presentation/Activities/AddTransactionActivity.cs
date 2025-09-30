using Android.App;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using MoneyTracker.Application.DTOs;
using MoneyTracker.Presentation.Base;
using MoneyTracker.Presentation.ViewModels;
using System;
using System.ComponentModel;

namespace MoneyTracker.Presentation.Activities
{
    [Activity(Label = "Add Transaction")]
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
            var spinnerCategory = FindViewById<Spinner>(Resource.Id.spinner_category)!;

            var set = CreateDataBindingSet();

            // ✅ FIX: Explicit ViewModel cast
            set.Bind(editDescription).To<string>(vm => ((AddTransactionViewModel)vm).Description).TwoWay();
            set.Bind(editAmount).To<decimal>(vm => ((AddTransactionViewModel)vm).Amount).TwoWay();
            set.Bind(editNotes).To<string>(vm => ((AddTransactionViewModel)vm).Notes).TwoWay();
            set.Bind(editLocation).To<string>(vm => ((AddTransactionViewModel)vm).Location).TwoWay();
            set.Bind(spinnerCategory)
                .To<CategoryDto?>(vm => ((AddTransactionViewModel)vm).SelectedCategory)
                .WithItemsSource<CategoryDto?>(vm => ((AddTransactionViewModel)vm).Categories)
                .TwoWay();

            // Commands
            set.Bind(buttonSave).To(vm => ((AddTransactionViewModel)vm).SaveTransactionCommand);
            set.Bind(buttonCancel).To(vm => ((AddTransactionViewModel)vm).CancelCommand);

            set.Apply();
            ApplyDataBindings();

#if DEBUG
            SetupSpinnerBindingManualTests(spinnerCategory);
#endif
        }

#if DEBUG
        private void SetupSpinnerBindingManualTests(Spinner spinner)
        {
            if (spinner == null)
            {
                return;
            }

            void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == nameof(AddTransactionViewModel.SelectedCategory))
                {
                    var selectedName = ViewModel.SelectedCategory?.Name ?? "(null)";
                    Log.Debug("AddTransactionActivity", $"ViewModel.SelectedCategory -> {selectedName}");
                }
            }

            void OnSpinnerItemSelected(object? sender, AdapterView.ItemSelectedEventArgs e)
            {
                var selected = spinner.Adapter?.GetItem(e.Position);
                Log.Debug("AddTransactionActivity", $"Spinner selection -> {selected}");
            }

            ViewModel.PropertyChanged += OnViewModelPropertyChanged;
            spinner.ItemSelected += OnSpinnerItemSelected;

            spinner.Post(() =>
            {
                if (ViewModel.Categories.Count > 1)
                {
                    ViewModel.SelectedCategory = ViewModel.Categories[1];
                }
            });

            void Cleanup()
            {
                ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
                spinner.ItemSelected -= OnSpinnerItemSelected;
            }

            spinner.AddOnAttachStateChangeListener(new SpinnerTestCleanupListener(Cleanup));
        }

        private class SpinnerTestCleanupListener : Java.Lang.Object, View.IOnAttachStateChangeListener
        {
            private readonly Action _cleanup;

            public SpinnerTestCleanupListener(Action cleanup)
            {
                _cleanup = cleanup;
            }

            public void OnViewAttachedToWindow(View attachedView)
            {
            }

            public void OnViewDetachedFromWindow(View detachedView)
            {
                _cleanup();
                detachedView.RemoveOnAttachStateChangeListener(this);
            }
        }
#endif
    }
}

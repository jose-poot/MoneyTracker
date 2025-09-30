using System;
using Android.App;
using Android.OS;
using Android.Widget;
using MoneyTracker.Presentation.Base;
using MoneyTracker.Presentation.ViewModels;

namespace MoneyTracker.Presentation.Activities;

[Activity(Label = "Settings")]
public sealed class SettingsActivity : ActivityBase<SettingsViewModel>
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.activity_settings);

        var txtWarehouses = FindRequired<TextView>(Resource.Id.txtWarehousesList);
        var editWarehouse = FindRequired<EditText>(Resource.Id.editSelectedWarehouse);
        var chkShowZones = FindRequired<CheckBox>(Resource.Id.chkShowZones);
        var txtZones = FindRequired<TextView>(Resource.Id.txtZonesList);
        var editZone = FindRequired<EditText>(Resource.Id.editSelectedZone);
        var txtReceivingBins = FindRequired<TextView>(Resource.Id.txtReceivingBinsList);
        var editReceivingBin = FindRequired<EditText>(Resource.Id.editSelectedReceivingBin);
        var txtShippingBins = FindRequired<TextView>(Resource.Id.txtShippingBinsList);
        var editShippingBin = FindRequired<EditText>(Resource.Id.editSelectedShippingBin);
        var chkShowPrinters = FindRequired<CheckBox>(Resource.Id.chkShowPrinters);
        var txtPrinters = FindRequired<TextView>(Resource.Id.txtPrintersList);
        var editPrinter = FindRequired<EditText>(Resource.Id.editSelectedPrinter);
        var txtQuickCountTypes = FindRequired<TextView>(Resource.Id.txtQuickCountTypesList);
        var editQuickCount = FindRequired<EditText>(Resource.Id.editSelectedQuickCountType);
        var btnRefresh = FindRequired<Button>(Resource.Id.btnRefreshSettings);
        var btnSave = FindRequired<Button>(Resource.Id.btnSaveSettings);

        var set = CreateDataBindingSet();

        set.Bind(txtWarehouses).To<string>(vm => ((SettingsViewModel)vm).WarehousesSummary);
        set.Bind(editWarehouse).To<string>(vm => ((SettingsViewModel)vm).SelectedWarehouseId).TwoWay();
        set.Bind(chkShowZones).To<bool>(vm => ((SettingsViewModel)vm).ShowZones).TwoWay();
        set.Bind(txtZones).To<string>(vm => ((SettingsViewModel)vm).ZonesSummary);
        set.Bind(editZone).To<string>(vm => ((SettingsViewModel)vm).SelectedZone).TwoWay();
        set.Bind(txtReceivingBins).To<string>(vm => ((SettingsViewModel)vm).ReceivingBinsSummary);
        set.Bind(editReceivingBin).To<string>(vm => ((SettingsViewModel)vm).SelectedReceivingBin).TwoWay();
        set.Bind(txtShippingBins).To<string>(vm => ((SettingsViewModel)vm).ShippingBinsSummary);
        set.Bind(editShippingBin).To<string>(vm => ((SettingsViewModel)vm).SelectedShippingBin).TwoWay();
        set.Bind(chkShowPrinters).To<bool>(vm => ((SettingsViewModel)vm).ShowPrinters).TwoWay();
        set.Bind(txtPrinters).To<string>(vm => ((SettingsViewModel)vm).PrintersSummary);
        set.Bind(editPrinter).To<string>(vm => ((SettingsViewModel)vm).SelectedPrinterName).TwoWay();
        set.Bind(txtQuickCountTypes).To<string>(vm => ((SettingsViewModel)vm).QuickCountTypesSummary);
        set.Bind(editQuickCount).To<string>(vm => ((SettingsViewModel)vm).SelectedQuickCountType).TwoWay();

        set.Bind(btnRefresh).To(vm => ((SettingsViewModel)vm).DoRefreshCommand);
        set.Bind(btnSave).To(vm => ((SettingsViewModel)vm).DoSaveCommand);

        set.Apply();
        ApplyDataBindings();

        _ = ViewModel.DoLoadCommand.ExecuteAsync(null);
    }

    private TView FindRequired<TView>(int resourceId) where TView : class
    {
        return FindViewById(resourceId) as TView
            ?? throw new InvalidOperationException($"The view with id {resourceId} was not found in the layout.");
    }
}

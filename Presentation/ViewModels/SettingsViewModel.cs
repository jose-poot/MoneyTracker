using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoneyTracker.Application.DTOs;
using MoneyTracker.Application.Services.Interfaces;
using MoneyTracker.Presentation.Services.Interfaces;

namespace MoneyTracker.Presentation.ViewModels;

/// <summary>
/// ViewModel that exposes configuration data for warehouse related settings.
/// </summary>
public partial class SettingsViewModel : BaseViewModel
{
    private readonly ICommonQueryService _commonQueryService;
    private readonly ISettingsService _settingsService;
    private readonly IAcumaticaOptionsProvider _optionsProvider;
    private bool _suppressWarehouseSelection;

    [ObservableProperty]
    private ObservableCollection<string> _warehouseIds = new();

    [ObservableProperty]
    private ObservableCollection<string> _zones = new();

    [ObservableProperty]
    private ObservableCollection<string> _receivingBins = new();

    [ObservableProperty]
    private ObservableCollection<string> _shippingBins = new();

    [ObservableProperty]
    private ObservableCollection<string> _printersName = new();

    [ObservableProperty]
    private ObservableCollection<string> _quickCountTypes = new();

    [ObservableProperty]
    private string? _selectedWarehouseId;

    [ObservableProperty]
    private string? _selectedZone;

    [ObservableProperty]
    private string? _selectedReceivingBin;

    [ObservableProperty]
    private string? _selectedShippingBin;

    [ObservableProperty]
    private string? _selectedPrinterName;

    [ObservableProperty]
    private string? _selectedQuickCountType;

    [ObservableProperty]
    private bool _showZones;

    [ObservableProperty]
    private bool _showPrinters;

    public SettingsViewModel(
        ICommonQueryService commonQueryService,
        ISettingsService settingsService,
        IAcumaticaOptionsProvider optionsProvider,
        IDialogService dialogService,
        INavigationService navigationService)
        : base(dialogService, navigationService)
    {
        _commonQueryService = commonQueryService;
        _settingsService = settingsService;
        _optionsProvider = optionsProvider;

        Title = "Warehouse Settings";
    }

    public string WarehousesSummary => FormatSummary(WarehouseIds);

    public string ZonesSummary => FormatSummary(Zones);

    public string ReceivingBinsSummary => FormatSummary(ReceivingBins);

    public string ShippingBinsSummary => FormatSummary(ShippingBins);

    public string PrintersSummary => FormatSummary(PrintersName);

    public string QuickCountTypesSummary => FormatSummary(QuickCountTypes);

    public bool HasZones => Zones.Count > 0;

    public bool HasPrinters => PrintersName.Count > 0;

    public bool HasQuickCountTypes => QuickCountTypes.Count > 0;

    [RelayCommand]
    private Task DoLoadAsync() => ExecuteSafeAsync(LoadInternalAsync);

    [RelayCommand]
    private Task DoRefreshAsync() => ExecuteSafeAsync(LoadInternalAsync);

    [RelayCommand]
    private Task DoSaveAsync() => ExecuteSafeAsync(SaveInternalAsync);

    private async Task LoadInternalAsync()
    {
        var warehouses = await _commonQueryService.GetWarehouseIdsAsync().ConfigureAwait(false);
        WarehouseIds = new ObservableCollection<string>(warehouses);
        OnPropertyChanged(nameof(WarehousesSummary));

        var printers = await _commonQueryService.GetPrintersAsync().ConfigureAwait(false);
        PrintersName = new ObservableCollection<string>(printers);
        OnPropertyChanged(nameof(PrintersSummary));
        OnPropertyChanged(nameof(HasPrinters));

        var quickCountTypes = _optionsProvider.GetQuickCountTypes();
        QuickCountTypes = new ObservableCollection<string>(quickCountTypes);
        OnPropertyChanged(nameof(QuickCountTypesSummary));
        OnPropertyChanged(nameof(HasQuickCountTypes));

        var dto = await _settingsService.GetWarehouseSettingsAsync().ConfigureAwait(false);

        _suppressWarehouseSelection = true;
        SelectedWarehouseId = !string.IsNullOrWhiteSpace(dto?.WarehouseId)
            ? dto!.WarehouseId
            : WarehouseIds.FirstOrDefault();
        SelectedZone = dto?.Zone;
        SelectedReceivingBin = dto?.ReceivingBin;
        SelectedShippingBin = dto?.ShippingBin;
        SelectedPrinterName = !string.IsNullOrWhiteSpace(dto?.PrinterName)
            ? dto!.PrinterName
            : PrintersName.FirstOrDefault();
        SelectedQuickCountType = !string.IsNullOrWhiteSpace(dto?.QuickCountType)
            ? dto!.QuickCountType
            : QuickCountTypes.FirstOrDefault();
        ShowZones = dto?.ShowZones ?? _optionsProvider.ShouldShowZones(SelectedWarehouseId ?? string.Empty);
        ShowPrinters = dto?.ShowPrinters ?? _optionsProvider.ShouldShowPrinters();
        _suppressWarehouseSelection = false;

        await SetWarehouse(SelectedWarehouseId).ConfigureAwait(false);
    }

    private async Task SaveInternalAsync()
    {
        var warehouseId = SelectedWarehouseId?.Trim();
        if (string.IsNullOrWhiteSpace(warehouseId))
        {
            if (DialogService != null)
            {
                await DialogService.ShowErrorAsync("Please provide a warehouse identifier.").ConfigureAwait(false);
            }
            return;
        }

        var settings = new WarehouseSettingsDto
        {
            WarehouseId = warehouseId,
            Zone = string.IsNullOrWhiteSpace(SelectedZone) ? null : SelectedZone!.Trim(),
            ReceivingBin = string.IsNullOrWhiteSpace(SelectedReceivingBin) ? null : SelectedReceivingBin!.Trim(),
            ShippingBin = string.IsNullOrWhiteSpace(SelectedShippingBin) ? null : SelectedShippingBin!.Trim(),
            PrinterName = string.IsNullOrWhiteSpace(SelectedPrinterName) ? null : SelectedPrinterName!.Trim(),
            QuickCountType = string.IsNullOrWhiteSpace(SelectedQuickCountType)
                ? QuickCountTypes.FirstOrDefault()
                : SelectedQuickCountType!.Trim(),
            ShowZones = ShowZones && HasZones,
            ShowPrinters = ShowPrinters && HasPrinters
        };

        await _settingsService.SaveWarehouseSettingsAsync(settings).ConfigureAwait(false);
        DialogService?.ShowToast("Settings saved successfully.");
    }

    private async Task SetWarehouse(string? warehouseId)
    {
        var metadataId = string.IsNullOrWhiteSpace(warehouseId)
            ? null
            : warehouseId!.Trim();

        var zones = metadataId == null
            ? Array.Empty<string>()
            : await _commonQueryService.GetZonesAsync(metadataId).ConfigureAwait(false);
        Zones = new ObservableCollection<string>(zones);
        OnPropertyChanged(nameof(ZonesSummary));
        OnPropertyChanged(nameof(HasZones));

        var receivingBins = metadataId == null
            ? Array.Empty<string>()
            : await _commonQueryService.GetReceivingBinsAsync(metadataId).ConfigureAwait(false);
        ReceivingBins = new ObservableCollection<string>(receivingBins);
        OnPropertyChanged(nameof(ReceivingBinsSummary));

        var shippingBins = metadataId == null
            ? Array.Empty<string>()
            : await _commonQueryService.GetShippingBinsAsync(metadataId).ConfigureAwait(false);
        ShippingBins = new ObservableCollection<string>(shippingBins);
        OnPropertyChanged(nameof(ShippingBinsSummary));

        if (!Zones.Contains(SelectedZone ?? string.Empty))
        {
            SelectedZone = Zones.FirstOrDefault();
        }

        if (!ReceivingBins.Contains(SelectedReceivingBin ?? string.Empty))
        {
            SelectedReceivingBin = ReceivingBins.FirstOrDefault();
        }

        if (!ShippingBins.Contains(SelectedShippingBin ?? string.Empty))
        {
            SelectedShippingBin = ShippingBins.FirstOrDefault();
        }

        if (metadataId != null)
        {
            ShowZones = ShowZones || _optionsProvider.ShouldShowZones(metadataId);
        }

        OnPropertyChanged(nameof(ShowZones));
    }

    partial void OnSelectedWarehouseIdChanged(string? value)
    {
        if (_suppressWarehouseSelection)
        {
            return;
        }

        _ = ExecuteSafeAsync(() => SetWarehouse(value), nameof(SetWarehouse));
    }

    private static string FormatSummary(ObservableCollection<string> values)
    {
        return values.Count switch
        {
            0 => "No data available",
            1 => values[0],
            _ => string.Join(", ", values)
        };
    }
}

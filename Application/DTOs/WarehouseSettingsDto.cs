namespace MoneyTracker.Application.DTOs;

public sealed class WarehouseSettingsDto
{
    public string? WarehouseId { get; set; }

    public string? Zone { get; set; }

    public string? ReceivingBin { get; set; }

    public string? ShippingBin { get; set; }

    public string? PrinterName { get; set; }

    public string? QuickCountType { get; set; }

    public bool ShowZones { get; set; }

    public bool ShowPrinters { get; set; }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using MoneyTracker.Application.DTOs;
using MoneyTracker.Application.Services.Interfaces;

namespace MoneyTracker.Infrastructure.Services;

public sealed class InMemorySettingsService : ISettingsService
{
    private readonly SemaphoreSlim _sync = new(1, 1);
    private WarehouseSettingsDto? _settings;

    public async Task<WarehouseSettingsDto?> GetWarehouseSettingsAsync(CancellationToken cancellationToken = default)
    {
        await _sync.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return _settings == null ? null : Clone(_settings);
        }
        finally
        {
            _sync.Release();
        }
    }

    public async Task SaveWarehouseSettingsAsync(WarehouseSettingsDto settings, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);

        await _sync.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            _settings = Clone(settings);
        }
        finally
        {
            _sync.Release();
        }
    }

    private static WarehouseSettingsDto Clone(WarehouseSettingsDto source)
        => new()
        {
            WarehouseId = source.WarehouseId,
            Zone = source.Zone,
            ReceivingBin = source.ReceivingBin,
            ShippingBin = source.ShippingBin,
            PrinterName = source.PrinterName,
            QuickCountType = source.QuickCountType,
            ShowZones = source.ShowZones,
            ShowPrinters = source.ShowPrinters
        };
}

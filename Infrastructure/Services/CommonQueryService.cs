using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MoneyTracker.Application.Services.Interfaces;

namespace MoneyTracker.Infrastructure.Services;

public sealed class CommonQueryService : ICommonQueryService
{
    private readonly IReadOnlyDictionary<string, WarehouseMetadata> _warehouses;
    private readonly IReadOnlyList<string> _printers;

    public CommonQueryService()
    {
        _warehouses = new Dictionary<string, WarehouseMetadata>(StringComparer.OrdinalIgnoreCase)
        {
            ["MAIN"] = new WarehouseMetadata(
                new List<string> { "A", "B" },
                new List<string> { "R1", "R2", "R3" },
                new List<string> { "S1", "S2" }),
            ["SECONDARY"] = new WarehouseMetadata(
                new List<string> { "NORTH", "SOUTH" },
                new List<string> { "RN1" },
                new List<string> { "SN1", "SN2", "SN3" }),
            ["OUTLET"] = new WarehouseMetadata(
                new List<string>(),
                new List<string> { "OR1" },
                new List<string> { "OS1" })
        };

        _printers = new List<string> { "PRT-01", "PRT-02", "PRT-03" };
    }

    public Task<IReadOnlyList<string>> GetWarehouseIdsAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<string> result = _warehouses.Keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase).ToList();
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<string>> GetZonesAsync(string warehouseId, CancellationToken cancellationToken = default)
    {
        var zones = ResolveWarehouse(warehouseId).Zones;
        return Task.FromResult<IReadOnlyList<string>>(zones);
    }

    public Task<IReadOnlyList<string>> GetReceivingBinsAsync(string warehouseId, CancellationToken cancellationToken = default)
    {
        var bins = ResolveWarehouse(warehouseId).ReceivingBins;
        return Task.FromResult<IReadOnlyList<string>>(bins);
    }

    public Task<IReadOnlyList<string>> GetShippingBinsAsync(string warehouseId, CancellationToken cancellationToken = default)
    {
        var bins = ResolveWarehouse(warehouseId).ShippingBins;
        return Task.FromResult<IReadOnlyList<string>>(bins);
    }

    public Task<IReadOnlyList<string>> GetPrintersAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_printers);

    private WarehouseMetadata ResolveWarehouse(string warehouseId)
    {
        if (string.IsNullOrWhiteSpace(warehouseId))
        {
            return WarehouseMetadata.Empty;
        }

        return _warehouses.TryGetValue(warehouseId, out var metadata)
            ? metadata
            : WarehouseMetadata.Empty;
    }

    private sealed class WarehouseMetadata
    {
        public WarehouseMetadata(IReadOnlyList<string> zones, IReadOnlyList<string> receivingBins, IReadOnlyList<string> shippingBins)
        {
            Zones = zones ?? Array.Empty<string>();
            ReceivingBins = receivingBins ?? Array.Empty<string>();
            ShippingBins = shippingBins ?? Array.Empty<string>();
        }

        public static WarehouseMetadata Empty { get; } = new(Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>());

        public IReadOnlyList<string> Zones { get; }

        public IReadOnlyList<string> ReceivingBins { get; }

        public IReadOnlyList<string> ShippingBins { get; }
    }
}

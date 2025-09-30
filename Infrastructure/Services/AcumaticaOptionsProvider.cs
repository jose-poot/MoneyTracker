using System.Collections.Generic;
using MoneyTracker.Application.Services.Interfaces;

namespace MoneyTracker.Infrastructure.Services;

public sealed class AcumaticaOptionsProvider : IAcumaticaOptionsProvider
{
    private static readonly IReadOnlyList<string> _quickCountTypes = new List<string>
    {
        "Cycle Count",
        "Full Count",
        "Spot Check"
    };

    public IReadOnlyList<string> GetQuickCountTypes() => _quickCountTypes;

    public bool ShouldShowZones(string warehouseId)
    {
        return !string.IsNullOrWhiteSpace(warehouseId);
    }

    public bool ShouldShowPrinters() => true;
}

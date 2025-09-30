using System.Collections.Generic;

namespace MoneyTracker.Application.Services.Interfaces;

public interface IAcumaticaOptionsProvider
{
    IReadOnlyList<string> GetQuickCountTypes();

    bool ShouldShowZones(string warehouseId);

    bool ShouldShowPrinters();
}

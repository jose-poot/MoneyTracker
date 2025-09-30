using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MoneyTracker.Application.Services.Interfaces;

public interface ICommonQueryService
{
    Task<IReadOnlyList<string>> GetWarehouseIdsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetZonesAsync(string warehouseId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetReceivingBinsAsync(string warehouseId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetShippingBinsAsync(string warehouseId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetPrintersAsync(CancellationToken cancellationToken = default);
}

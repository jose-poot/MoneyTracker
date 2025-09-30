using System.Threading;
using System.Threading.Tasks;
using MoneyTracker.Application.DTOs;

namespace MoneyTracker.Application.Services.Interfaces;

public interface ISettingsService
{
    Task<WarehouseSettingsDto?> GetWarehouseSettingsAsync(CancellationToken cancellationToken = default);

    Task SaveWarehouseSettingsAsync(WarehouseSettingsDto settings, CancellationToken cancellationToken = default);
}

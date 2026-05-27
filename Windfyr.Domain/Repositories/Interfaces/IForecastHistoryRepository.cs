using Windfyr.Domain.Entities;

namespace Windfyr.Domain.Repositories.Interfaces;

public interface IForecastHistoryRepository
{
    Task<ForecastHistory?> GetLatestAsync(ulong landId, CancellationToken ct);
    Task AddAsync(ForecastHistory history, CancellationToken ct);
}

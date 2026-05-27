using Windfyr.Domain.Entities;

namespace Windfyr.Domain.Repositories;

public interface IFireAlertRepository
{
    Task AddAsync(FireAlert alert, CancellationToken cancellationToken = default);
    Task BulkAddAsync(IEnumerable<FireAlert> alerts, CancellationToken cancellationToken = default);
    Task<IEnumerable<FireAlert>> GetActiveAlertsAsync(CancellationToken cancellationToken = default);
}
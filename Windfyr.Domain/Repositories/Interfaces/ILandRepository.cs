using Windfyr.Domain.Entities;

namespace Windfyr.Domain.Repositories.Interfaces;

public interface ILandRepository
{
    Task<IReadOnlyList<Land>> GetAllByUserAsync(ulong userId, CancellationToken ct);
    Task<Land?> GetByIdAsync(ulong userId, ulong landId, CancellationToken ct);
    Task AddAsync(Land land, CancellationToken ct);
    Task UpdateAsync(Land land, CancellationToken ct);
    Task DeleteAsync(Land land, CancellationToken ct);
}

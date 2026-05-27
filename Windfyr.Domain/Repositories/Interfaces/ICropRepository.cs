using Windfyr.Domain.Entities;

namespace Windfyr.Domain.Repositories.Interfaces;

public interface ICropRepository
{
    Task<IReadOnlyList<Crop>> GetAllAsync(CancellationToken ct);
    Task<bool> ExistsByIdAsync(ulong id, CancellationToken ct);
}

namespace Windfyr.Domain.Repositories.Interfaces;

public interface IIrrigationHistoryRepository
{
    Task<string?> GetLatestStatusAsync(ulong landId, CancellationToken ct);
    Task AddAsync(Windfyr.Domain.Entities.IrrigationHistory history, CancellationToken ct);
}

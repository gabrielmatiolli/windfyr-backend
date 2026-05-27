using Microsoft.EntityFrameworkCore;
using Windfyr.Domain.Entities;
using Windfyr.Domain.Repositories.Interfaces;
using Windfyr.Repository.Context;

namespace Windfyr.Repository.Repositories;

public sealed class IrrigationHistoryRepository(WindfyrDbContext dbContext) : IIrrigationHistoryRepository
{
    public async Task<string?> GetLatestStatusAsync(ulong landId, CancellationToken ct)
    {
        return await dbContext.IrrigationHistories
            .AsNoTracking()
            .Where(history => history.LandId == landId)
            .OrderByDescending(history => history.Date)
            .Select(history => history.Status)
            .FirstOrDefaultAsync(ct);
    }

    public async Task AddAsync(IrrigationHistory history, CancellationToken ct)
    {
        dbContext.IrrigationHistories.Add(history);
        await dbContext.SaveChangesAsync(ct);
    }
}

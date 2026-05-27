using Microsoft.EntityFrameworkCore;
using Windfyr.Domain.Entities;
using Windfyr.Domain.Repositories.Interfaces;
using Windfyr.Repository.Context;

namespace Windfyr.Repository.Repositories;

public sealed class ForecastHistoryRepository(WindfyrDbContext dbContext) : IForecastHistoryRepository
{
    public Task<ForecastHistory?> GetLatestAsync(ulong landId, CancellationToken ct)
    {
        return dbContext.ForecastHistories
            .AsNoTracking()
            .Where(history => history.LandId == landId)
            .OrderByDescending(history => history.Date)
            .FirstOrDefaultAsync(ct);
    }

    public async Task AddAsync(ForecastHistory history, CancellationToken ct)
    {
        dbContext.ForecastHistories.Add(history);
        await dbContext.SaveChangesAsync(ct);
    }
}

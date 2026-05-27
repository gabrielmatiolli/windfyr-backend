using Microsoft.EntityFrameworkCore;
using Windfyr.Domain.Entities;
using Windfyr.Domain.Repositories.Interfaces;
using Windfyr.Repository.Context;

namespace Windfyr.Repository.Repositories;

public sealed class CropRepository(WindfyrDbContext dbContext) : ICropRepository
{
    public async Task<IReadOnlyList<Crop>> GetAllAsync(CancellationToken ct)
    {
        return await dbContext.Crops
            .AsNoTracking()
            .OrderBy(crop => crop.Type)
            .ToListAsync(ct);
    }

    public Task<bool> ExistsByIdAsync(ulong id, CancellationToken ct)
    {
        return dbContext.Crops
            .AsNoTracking()
            .AnyAsync(crop => crop.Id == id, ct);
    }
}

using Microsoft.EntityFrameworkCore;
using Windfyr.Domain.Entities;
using Windfyr.Domain.Repositories.Interfaces;
using Windfyr.Repository.Context;

namespace Windfyr.Repository.Repositories;

public sealed class UserRepository(WindfyrDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByIdAsync(ulong id, CancellationToken ct)
    {
        return dbContext.Users
            .FirstOrDefaultAsync(user => user.Id == id, ct);
    }

    public Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken ct)
    {
        return dbContext.Users
            .FirstOrDefaultAsync(user => user.Email == normalizedEmail, ct);
    }

    public Task<bool> ExistsByEmailAsync(string normalizedEmail, CancellationToken ct)
    {
        return dbContext.Users
            .AsNoTracking()
            .AnyAsync(user => user.Email == normalizedEmail, ct);
    }

    public async Task AddAsync(User user, CancellationToken ct)
    {
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(ct);
    }

    public Task UpdateAsync(User user, CancellationToken ct)
    {
        dbContext.Users.Update(user);
        return dbContext.SaveChangesAsync(ct);
    }
}

using Windfyr.Domain.Entities;

namespace Windfyr.Domain.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(ulong id, CancellationToken ct);
    Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken ct);
    Task<bool> ExistsByEmailAsync(string normalizedEmail, CancellationToken ct);
    Task AddAsync(User user, CancellationToken ct);
    Task UpdateAsync(User user, CancellationToken ct);
}

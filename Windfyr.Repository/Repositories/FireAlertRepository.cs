using Microsoft.EntityFrameworkCore;
using Windfyr.Domain.Entities;
using Windfyr.Domain.Repositories;
using Windfyr.Repository.Context;

namespace Windfyr.Repository.Repositories
{
    public class FireAlertRepository : IFireAlertRepository
    {
        private readonly WindfyrDbContext _context;

        public FireAlertRepository(WindfyrDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(FireAlert alert, CancellationToken cancellationToken = default)
        {
            await _context.FireAlerts.AddAsync(alert, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task BulkAddAsync(IEnumerable<FireAlert> alerts, CancellationToken cancellationToken = default)
        {
            await _context.FireAlerts.AddRangeAsync(alerts, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<FireAlert>> GetActiveAlertsAsync(CancellationToken cancellationToken = default)
        {
            // AsNoTracking melhora muito a performance em consultas de leitura (usadas pela API do Kotlin)
            return await _context.FireAlerts
                .AsNoTracking()
                .Where(x => x.DetectedAt >= DateTime.UtcNow.AddDays(-2)) // Exemplo: focos das últimas 48h
                .ToListAsync(cancellationToken);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Windfyr.Domain.Entities;

namespace Windfyr.Repository.Context
{
    public class WindfyrDbContext : DbContext
    {
        public WindfyrDbContext(DbContextOptions<WindfyrDbContext> options) : base(options) { }

        public DbSet<FireAlert> FireAlerts => Set<FireAlert>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configurações de banco de dados (Tabelas, índices, chaves)
            modelBuilder.Entity<FireAlert>(builder =>
            {
                builder.ToTable("FireAlerts");
                builder.HasKey(x => x.Id);
                builder.HasIndex(x => new { x.Latitude, x.Longitude }); // Índice para buscas geográficas rápidas
            });
        }
    }
}

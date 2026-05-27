using Microsoft.EntityFrameworkCore;
using Windfyr.Domain.Entities;

namespace Windfyr.Repository.Context
{
    public class WindfyrDbContext : DbContext
    {
        public WindfyrDbContext(DbContextOptions<WindfyrDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Crop> Crops => Set<Crop>();
        public DbSet<Land> Lands => Set<Land>();
        public DbSet<ForecastHistory> ForecastHistories => Set<ForecastHistory>();
        public DbSet<IrrigationHistory> IrrigationHistories => Set<IrrigationHistory>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(builder =>
            {
                builder.ToTable("users");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
                builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
                builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
                builder.Property(x => x.Password).HasColumnName("password").HasMaxLength(255).IsRequired();
                builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
                builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

                builder.HasIndex(x => x.Email).IsUnique().HasDatabaseName("uq_users_email");
            });

            modelBuilder.Entity<Crop>(builder =>
            {
                builder.ToTable("crops");
                builder.HasKey(x => x.Id);

                builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
                builder.Property(x => x.Type).HasColumnName("type").HasMaxLength(100).IsRequired();
                builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
                builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();
            });

            modelBuilder.Entity<Land>(builder =>
            {
                builder.ToTable("lands");
                builder.HasKey(x => x.Id);

                builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
                builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
                builder.Property(x => x.CropId).HasColumnName("crop_id").IsRequired();
                builder.Property(x => x.Hectares).HasColumnName("hectares").HasPrecision(10, 2).IsRequired();
                builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
                builder.Property(x => x.State).HasColumnName("state").HasMaxLength(100).IsRequired();
                builder.Property(x => x.Neighborhood).HasColumnName("neighborhood").HasMaxLength(120);
                builder.Property(x => x.City).HasColumnName("city").HasMaxLength(120).IsRequired();
                builder.Property(x => x.Street).HasColumnName("street").HasMaxLength(150);
                builder.Property(x => x.Number).HasColumnName("number").HasMaxLength(20);
                builder.Property(x => x.Latitude).HasColumnName("latitude").HasPrecision(10, 8).IsRequired();
                builder.Property(x => x.Longitude).HasColumnName("longitude").HasPrecision(11, 8).IsRequired();
                builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
                builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

                builder.HasIndex(x => x.UserId).HasDatabaseName("idx_lands_user_id");
                builder.HasIndex(x => x.CropId).HasDatabaseName("idx_lands_crop_id");

                builder.HasOne(x => x.User)
                    .WithMany(x => x.Lands)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("fk_lands_user");

                builder.HasOne(x => x.Crop)
                    .WithMany(x => x.Lands)
                    .HasForeignKey(x => x.CropId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("fk_lands_crop");
            });

            modelBuilder.Entity<ForecastHistory>(builder =>
            {
                builder.ToTable("forecast_histories");
                builder.HasKey(x => x.Id);

                builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
                builder.Property(x => x.LandId).HasColumnName("land_id").IsRequired();
                builder.Property(x => x.Date).HasColumnName("date").IsRequired();
                builder.Property(x => x.AirTemperature).HasColumnName("air_temperature").HasPrecision(5, 2).IsRequired();
                builder.Property(x => x.AirPressure).HasColumnName("air_pressure").HasPrecision(7, 2).IsRequired();
                builder.Property(x => x.AirHumidity).HasColumnName("air_humidity").HasPrecision(5, 2).IsRequired();
                builder.Property(x => x.RegionFires).HasColumnName("region_fires").HasDefaultValue(0).IsRequired();
                builder.Property(x => x.RegionFrosts).HasColumnName("region_frosts").HasDefaultValue(0).IsRequired();
                builder.Property(x => x.RegionLightnings).HasColumnName("region_lightnings").HasDefaultValue(0).IsRequired();
                builder.Property(x => x.RegionFireRiskPercent).HasColumnName("region_fire_risk_percent").HasPrecision(5, 2).IsRequired();
                builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
                builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

                builder.HasIndex(x => x.LandId).HasDatabaseName("idx_forecast_histories_land_id");
                builder.HasIndex(x => x.Date).HasDatabaseName("idx_forecast_histories_date");

                builder.HasOne(x => x.Land)
                    .WithMany(x => x.ForecastHistories)
                    .HasForeignKey(x => x.LandId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("fk_forecast_histories_land");
            });

            modelBuilder.Entity<IrrigationHistory>(builder =>
            {
                builder.ToTable("irrigation_histories");
                builder.HasKey(x => x.Id);

                builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
                builder.Property(x => x.LandId).HasColumnName("land_id").IsRequired();
                builder.Property(x => x.Date).HasColumnName("date").IsRequired();
                builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
                builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
                builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

                builder.HasIndex(x => x.LandId).HasDatabaseName("idx_irrigation_histories_land_id");
                builder.HasIndex(x => x.Date).HasDatabaseName("idx_irrigation_histories_date");

                builder.HasOne(x => x.Land)
                    .WithMany(x => x.IrrigationHistories)
                    .HasForeignKey(x => x.LandId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("fk_irrigation_histories_land");
            });
        }
    }
}

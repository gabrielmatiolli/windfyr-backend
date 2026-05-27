using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Windfyr.Domain.Entities;
using Windfyr.Domain.Http.Interfaces;
using Windfyr.Domain.Http.Models;
using Windfyr.Repository.Context;

namespace Windfyr.WebApi.Tests.Infrastructure;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<WindfyrDbContext>>();
            _connection.Open();
            services.AddDbContext<WindfyrDbContext>(options => options.UseSqlite(_connection));

            services.RemoveAll(typeof(IOpenWeatherClient));
            services.AddSingleton<IOpenWeatherClient>(new FakeOpenWeatherClient());

            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            });

            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<WindfyrDbContext>();
            db.Database.EnsureCreated();

            if (!db.Crops.Any())
            {
                db.Crops.AddRange(
                    new Crop { Id = 1, Type = "Milho", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new Crop { Id = 2, Type = "Soja", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
            }

            if (!db.Users.Any())
            {
                db.Users.Add(new User
                {
                    Id = 1,
                    Name = "Joao",
                    Email = "joao@email.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("123456"),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            if (!db.Lands.Any())
            {
                db.Lands.Add(new Land
                {
                    Id = 1,
                    UserId = 1,
                    CropId = 1,
                    Name = "Fazenda A",
                    Hectares = 10,
                    State = "SP",
                    City = "Ribeirao Preto",
                    Latitude = -21.1m,
                    Longitude = -47.8m,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            db.SaveChanges();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }

    private sealed class FakeOpenWeatherClient : IOpenWeatherClient
    {
        public Task<OpenWeatherCurrentWeather> GetCurrentWeatherAsync(decimal latitude, decimal longitude, CancellationToken ct)
        {
            return Task.FromResult(new OpenWeatherCurrentWeather(
                TemperatureCelsius: 25,
                PressureHpa: 1010,
                HumidityPercent: 40,
                Description: "clear",
                Icon: "01d",
                DataTimeUtc: DateTimeOffset.UtcNow,
                CityName: "Ribeirao Preto"));
        }
    }
}

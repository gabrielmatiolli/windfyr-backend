using Microsoft.EntityFrameworkCore;
using Windfyr.Http.OpenWeather;
using Windfyr.Repository.Context;
using Windfyr.WebApi.Irrigation;

namespace Windfyr.WebApi.Forecast;

public interface IForecastService
{
    Task<CurrentForecastResponse> CaptureCurrentForecastAsync(ulong landId, ulong userId, CancellationToken ct);
    Task RefreshLandForecastHistoriesAsync(IEnumerable<Windfyr.Domain.Entities.Land> lands, CancellationToken ct);
}

public sealed class ForecastService(
    WindfyrDbContext dbContext,
    IOpenWeatherClient openWeatherClient,
    IIrrigationService irrigationService) : IForecastService
{
    public async Task<CurrentForecastResponse> CaptureCurrentForecastAsync(ulong landId, ulong userId, CancellationToken ct)
    {
        var land = await dbContext.Lands
            .Include(l => l.ForecastHistories)
            .Include(l => l.IrrigationHistories)
            .FirstOrDefaultAsync(l => l.Id == landId && l.UserId == userId, ct);

        if (land is null)
        {
            throw new KeyNotFoundException();
        }

        return await CaptureCurrentForecastAsync(land, ct);
    }

    public async Task RefreshLandForecastHistoriesAsync(IEnumerable<Windfyr.Domain.Entities.Land> lands, CancellationToken ct)
    {
        foreach (var land in lands)
        {
            await CaptureCurrentForecastAsync(land, ct);
        }
    }

    private async Task<CurrentForecastResponse> CaptureCurrentForecastAsync(Windfyr.Domain.Entities.Land land, CancellationToken ct)
    {
        var weather = await openWeatherClient.GetCurrentWeatherAsync(land.Latitude, land.Longitude, ct);
        var normalizedTemperature = decimal.Round(weather.TemperatureCelsius, 2);
        var normalizedPressure = decimal.Round(weather.PressureHpa, 2);
        var normalizedHumidity = decimal.Round(weather.HumidityPercent, 2);
        var fireRiskPercent = CalculateFireRiskPercent(
            normalizedTemperature,
            normalizedHumidity,
            normalizedPressure);

        irrigationService.ApplyRiskStatusTransition(land, weather.DataTimeUtc.UtcDateTime, fireRiskPercent);

        var latestHistory = land.ForecastHistories
            .OrderByDescending(history => history.Date)
            .FirstOrDefault();

        if (HasForecastChanged(latestHistory, normalizedTemperature, normalizedPressure, normalizedHumidity, fireRiskPercent))
        {
            var history = new Windfyr.Domain.Entities.ForecastHistory
            {
                LandId = land.Id,
                Date = weather.DataTimeUtc.UtcDateTime,
                AirTemperature = normalizedTemperature,
                AirPressure = normalizedPressure,
                AirHumidity = normalizedHumidity,
                RegionFires = 0,
                RegionFrosts = 0,
                RegionLightnings = 0,
                RegionFireRiskPercent = fireRiskPercent,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            dbContext.ForecastHistories.Add(history);
            land.ForecastHistories.Add(history);
        }

        await dbContext.SaveChangesAsync(ct);

        return new CurrentForecastResponse(
            LandId: land.Id,
            LandName: land.Name,
            Latitude: land.Latitude,
            Longitude: land.Longitude,
            Forecast: new CurrentForecastDataResponse(
                TemperatureCelsius: normalizedTemperature,
                PressureHpa: normalizedPressure,
                HumidityPercent: normalizedHumidity,
                Description: weather.Description,
                Icon: weather.Icon,
                DataTimeUtc: weather.DataTimeUtc,
                CityName: weather.CityName,
                RegionFireRiskPercent: fireRiskPercent));
    }

    private static bool HasForecastChanged(
        Windfyr.Domain.Entities.ForecastHistory? latest,
        decimal airTemperature,
        decimal airPressure,
        decimal airHumidity,
        decimal fireRiskPercent)
    {
        if (latest is null)
        {
            return true;
        }

        return latest.AirTemperature != airTemperature
            || latest.AirPressure != airPressure
            || latest.AirHumidity != airHumidity
            || latest.RegionFireRiskPercent != fireRiskPercent;
    }

    private static decimal CalculateFireRiskPercent(
        decimal temperatureCelsius,
        decimal humidityPercent,
        decimal pressureHpa)
    {
        var temperatureScore = Clamp((temperatureCelsius - 20m) / 20m * 50m, 0m, 50m);
        var humidityScore = Clamp((60m - humidityPercent) / 60m * 35m, 0m, 35m);
        var pressureScore = Clamp((1015m - pressureHpa) / 25m * 15m, 0m, 15m);

        return decimal.Round(Clamp(temperatureScore + humidityScore + pressureScore, 0m, 100m), 2);
    }

    private static decimal Clamp(decimal value, decimal min, decimal max)
    {
        if (value < min)
        {
            return min;
        }

        return value > max ? max : value;
    }
}

using Windfyr.Domain.Entities;
using Windfyr.Domain.Http.Interfaces;
using Windfyr.Domain.Repositories.Interfaces;
using Windfyr.Domain.UseCases.Forecast.Inputs;
using Windfyr.Domain.UseCases.Forecast.Interfaces;
using Windfyr.Domain.UseCases.Forecast.Outputs;
using Windfyr.Domain.UseCases.Irrigation.Inputs;
using Windfyr.Domain.UseCases.Irrigation.Interfaces;

namespace Windfyr.Domain.UseCases.Forecast.Implementations;

public sealed class CaptureCurrentForecastUseCase(
    ILandRepository landRepository,
    IForecastHistoryRepository forecastHistoryRepository,
    IOpenWeatherClient openWeatherClient,
    IApplyIrrigationRiskUseCase irrigationRiskUseCase) : ICaptureCurrentForecastUseCase
{
    public async Task<CurrentForecastOutput> ExecuteAsync(CaptureCurrentForecastInput input, CancellationToken ct)
    {
        var land = await landRepository.GetByIdAsync(input.UserId, input.LandId, ct);
        if (land is null)
        {
            throw new KeyNotFoundException();
        }

        return await CaptureCurrentForecastAsync(land, ct);
    }

    public async Task<CurrentForecastOutput> CaptureCurrentForecastAsync(Windfyr.Domain.Entities.Land land, CancellationToken ct)
    {
        var weather = await openWeatherClient.GetCurrentWeatherAsync(land.Latitude, land.Longitude, ct);
        var normalizedTemperature = decimal.Round(weather.TemperatureCelsius, 2);
        var normalizedPressure = decimal.Round(weather.PressureHpa, 2);
        var normalizedHumidity = decimal.Round(weather.HumidityPercent, 2);
        var fireRiskPercent = CalculateFireRiskPercent(
            normalizedTemperature,
            normalizedHumidity,
            normalizedPressure);

        await irrigationRiskUseCase.ExecuteAsync(
            new ApplyIrrigationRiskInput(land, weather.DataTimeUtc.UtcDateTime, fireRiskPercent),
            ct);

        var latestHistory = await forecastHistoryRepository.GetLatestAsync(land.Id, ct);
        if (HasForecastChanged(latestHistory, normalizedTemperature, normalizedPressure, normalizedHumidity, fireRiskPercent))
        {
            var history = new ForecastHistory
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

            await forecastHistoryRepository.AddAsync(history, ct);
            land.ForecastHistories.Add(history);
        }

        return new CurrentForecastOutput(
            LandId: land.Id,
            LandName: land.Name,
            Latitude: land.Latitude,
            Longitude: land.Longitude,
            Forecast: new CurrentForecastDataOutput(
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
        ForecastHistory? latest,
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

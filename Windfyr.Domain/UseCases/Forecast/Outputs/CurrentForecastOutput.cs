namespace Windfyr.Domain.UseCases.Forecast.Outputs;

public sealed record CurrentForecastOutput(
    ulong LandId,
    string LandName,
    decimal Latitude,
    decimal Longitude,
    CurrentForecastDataOutput Forecast);

public sealed record CurrentForecastDataOutput(
    decimal TemperatureCelsius,
    decimal PressureHpa,
    decimal HumidityPercent,
    string Description,
    string Icon,
    DateTimeOffset DataTimeUtc,
    string CityName,
    decimal RegionFireRiskPercent);

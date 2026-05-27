namespace Windfyr.WebApi.Forecast;

public sealed record CurrentForecastDataResponse(
    decimal TemperatureCelsius,
    decimal PressureHpa,
    decimal HumidityPercent,
    string Description,
    string Icon,
    DateTimeOffset DataTimeUtc,
    string CityName,
    decimal RegionFireRiskPercent);

public sealed record CurrentForecastResponse(
    ulong LandId,
    string LandName,
    decimal Latitude,
    decimal Longitude,
    CurrentForecastDataResponse Forecast);

namespace Windfyr.Domain.Http.Models;

public sealed record OpenWeatherCurrentWeather(
    decimal TemperatureCelsius,
    decimal PressureHpa,
    decimal HumidityPercent,
    string Description,
    string Icon,
    DateTimeOffset DataTimeUtc,
    string CityName);

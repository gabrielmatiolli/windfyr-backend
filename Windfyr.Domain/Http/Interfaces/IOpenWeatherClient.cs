using Windfyr.Domain.Http.Models;

namespace Windfyr.Domain.Http.Interfaces;

public interface IOpenWeatherClient
{
    Task<OpenWeatherCurrentWeather> GetCurrentWeatherAsync(decimal latitude, decimal longitude, CancellationToken ct);
}

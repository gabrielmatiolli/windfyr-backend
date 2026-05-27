using System.Globalization;
using System.Text.Json;

namespace Windfyr.Http.OpenWeather;

public interface IOpenWeatherClient
{
    Task<OpenWeatherCurrentWeather> GetCurrentWeatherAsync(decimal latitude, decimal longitude, CancellationToken ct);
}

public sealed class OpenWeatherClient(HttpClient httpClient, OpenWeatherSettings settings) : IOpenWeatherClient
{
    public async Task<OpenWeatherCurrentWeather> GetCurrentWeatherAsync(decimal latitude, decimal longitude, CancellationToken ct)
    {
        var latValue = latitude.ToString(CultureInfo.InvariantCulture);
        var lonValue = longitude.ToString(CultureInfo.InvariantCulture);

        var path = $"/data/2.5/weather?lat={latValue}&lon={lonValue}&units=metric&lang=pt_br&appid={settings.ApiKey}";

        using var response = await httpClient.GetAsync(path, ct);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"Erro ao consultar OpenWeatherMap: {(int)response.StatusCode} {response.ReasonPhrase}. {body}");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

        var root = json.RootElement;
        var main = root.GetProperty("main");

        var weatherArray = root.GetProperty("weather");
        var weather = weatherArray.GetArrayLength() > 0
            ? weatherArray[0]
            : default;

        var description = weather.ValueKind == JsonValueKind.Object && weather.TryGetProperty("description", out var descriptionProp)
            ? descriptionProp.GetString() ?? string.Empty
            : string.Empty;

        var icon = weather.ValueKind == JsonValueKind.Object && weather.TryGetProperty("icon", out var iconProp)
            ? iconProp.GetString() ?? string.Empty
            : string.Empty;

        var dtUnix = root.TryGetProperty("dt", out var dtProp) && dtProp.TryGetInt64(out var parsedDt)
            ? parsedDt
            : DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var cityName = root.TryGetProperty("name", out var nameProp)
            ? nameProp.GetString() ?? string.Empty
            : string.Empty;

        return new OpenWeatherCurrentWeather(
            TemperatureCelsius: main.GetProperty("temp").GetDecimal(),
            PressureHpa: main.GetProperty("pressure").GetDecimal(),
            HumidityPercent: main.GetProperty("humidity").GetDecimal(),
            Description: description,
            Icon: icon,
            DataTimeUtc: DateTimeOffset.FromUnixTimeSeconds(dtUnix),
            CityName: cityName);
    }
}

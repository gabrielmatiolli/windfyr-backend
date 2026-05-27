namespace Windfyr.Http.OpenWeather;

public sealed class OpenWeatherSettings
{
    public string ApiKey { get; init; } = string.Empty;
    public string BaseUrl { get; init; } = "https://api.openweathermap.org";

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            throw new InvalidOperationException("OPENWEATHER__APIKEY nao configurada no arquivo .env.");
        }

        if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out _))
        {
            throw new InvalidOperationException("OPENWEATHER__BASEURL invalida no arquivo .env.");
        }
    }
}

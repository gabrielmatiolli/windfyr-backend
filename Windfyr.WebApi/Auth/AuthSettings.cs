namespace Windfyr.WebApi.Auth;

public sealed class AuthSettings
{
    public string Key { get; init; } = string.Empty;
    public string Issuer { get; init; } = "windfyr-api";
    public string Audience { get; init; } = "windfyr-client";
    public int ExpiresInMinutes { get; init; } = 60;

    public static AuthSettings FromConfiguration(IConfiguration configuration)
    {
        var key = configuration["JWT:Key"]
            ?? Environment.GetEnvironmentVariable("JWT__Key")
            ?? string.Empty;

        var issuer = configuration["JWT:Issuer"]
            ?? Environment.GetEnvironmentVariable("JWT__Issuer")
            ?? "windfyr-api";

        var audience = configuration["JWT:Audience"]
            ?? Environment.GetEnvironmentVariable("JWT__Audience")
            ?? "windfyr-client";

        var expiresValue = configuration["JWT:ExpiresInMinutes"]
            ?? Environment.GetEnvironmentVariable("JWT__ExpiresInMinutes");

        var expiresInMinutes = 60;
        if (!string.IsNullOrWhiteSpace(expiresValue)
            && int.TryParse(expiresValue, out var parsedExpires)
            && parsedExpires > 0)
        {
            expiresInMinutes = parsedExpires;
        }

        return new AuthSettings
        {
            Key = key,
            Issuer = issuer,
            Audience = audience,
            ExpiresInMinutes = expiresInMinutes
        };
    }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Key) || Key.Length < 32)
        {
            throw new InvalidOperationException(
                "JWT__Key nao configurada (minimo de 32 caracteres) no arquivo .env.");
        }
    }
}

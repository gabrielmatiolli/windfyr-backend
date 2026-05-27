namespace Windfyr.Domain.UseCases.Auth.Inputs;

public sealed class AuthTokenSettings
{
    public string Key { get; init; } = string.Empty;
    public string Issuer { get; init; } = "windfyr-api";
    public string Audience { get; init; } = "windfyr-client";
    public int ExpiresInMinutes { get; init; } = 60;
}

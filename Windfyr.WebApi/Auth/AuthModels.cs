using System.Text.Json.Serialization;

namespace Windfyr.WebApi.Auth;

public sealed record LoginRequest(string Email, string Password);

public sealed record RegisterRequest(
    string Name,
    string Email,
    string Password,
    [property: JsonPropertyName("password_confirmation")] string PasswordConfirmation);

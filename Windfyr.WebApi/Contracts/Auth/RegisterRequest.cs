using System.Text.Json.Serialization;

namespace Windfyr.WebApi.Contracts.Auth;

public sealed record RegisterRequest(
    string Name,
    string Email,
    string Password,
    [property: JsonPropertyName("password_confirmation")] string PasswordConfirmation);

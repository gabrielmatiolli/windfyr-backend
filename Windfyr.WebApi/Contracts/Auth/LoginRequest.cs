namespace Windfyr.WebApi.Contracts.Auth;

public sealed record LoginRequest(string Email, string Password);

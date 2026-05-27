namespace Windfyr.WebApi.Contracts.User;

public sealed record UpdateCurrentUserRequest(
    string Name,
    string Email,
    string? Password = null,
    string? PasswordConfirmation = null);

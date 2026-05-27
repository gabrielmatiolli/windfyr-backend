namespace Windfyr.WebApi.User;

public sealed record UpdateUserProfileRequest(
	string Name,
	string Email,
	string? Password = null,
	string? PasswordConfirmation = null);

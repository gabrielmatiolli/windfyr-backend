namespace Windfyr.Domain.UseCases.User.Inputs;

public sealed record UpdateCurrentUserInput(
    ulong UserId,
    string Name,
    string Email,
    string? Password = null,
    string? PasswordConfirmation = null);

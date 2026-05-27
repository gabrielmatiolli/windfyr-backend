namespace Windfyr.Domain.UseCases.Auth.Inputs;

public sealed record RegisterUserInput(
    string Name,
    string Email,
    string Password,
    string PasswordConfirmation);

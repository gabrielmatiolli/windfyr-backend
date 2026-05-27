namespace Windfyr.Domain.UseCases.Auth.Inputs;

public sealed record LoginUserInput(string Email, string Password);

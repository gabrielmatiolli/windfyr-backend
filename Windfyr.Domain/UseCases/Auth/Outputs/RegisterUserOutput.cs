namespace Windfyr.Domain.UseCases.Auth.Outputs;

public sealed record RegisterUserOutput(
    string Message,
    RegisteredUserOutput User);

public sealed record RegisteredUserOutput(
    ulong Id,
    string Name,
    string Email);

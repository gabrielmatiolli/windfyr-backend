namespace Windfyr.Domain.UseCases.Auth.Outputs;

public sealed record LoginUserOutput(
    string Token,
    LoggedUserOutput User);

public sealed record LoggedUserOutput(
    ulong Id,
    string Name,
    string Email);

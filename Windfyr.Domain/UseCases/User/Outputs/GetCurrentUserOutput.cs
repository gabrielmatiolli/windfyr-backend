namespace Windfyr.Domain.UseCases.User.Outputs;

public sealed record GetCurrentUserOutput(
    ulong Id,
    string Name,
    string Email);

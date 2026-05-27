namespace Windfyr.Domain.UseCases.User.Outputs;

public sealed record UpdateCurrentUserOutput(
    string Message,
    UpdatedUserOutput User);

public sealed record UpdatedUserOutput(
    ulong Id,
    string Name,
    string Email);

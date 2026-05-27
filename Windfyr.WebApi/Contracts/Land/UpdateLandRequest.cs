namespace Windfyr.WebApi.Contracts.Land;

public sealed record UpdateLandRequest(
    string? Name,
    decimal? Hectares);

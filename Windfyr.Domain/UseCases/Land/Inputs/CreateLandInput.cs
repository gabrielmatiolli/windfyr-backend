namespace Windfyr.Domain.UseCases.Land.Inputs;

public sealed record CreateLandInput(
    ulong UserId,
    ulong CropId,
    string Name,
    decimal Hectares,
    string State,
    string City,
    string? Neighborhood,
    string? Street,
    string? Number,
    decimal Latitude,
    decimal Longitude);

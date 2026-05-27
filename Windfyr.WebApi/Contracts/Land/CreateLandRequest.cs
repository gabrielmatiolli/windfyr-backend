using System.Text.Json.Serialization;

namespace Windfyr.WebApi.Contracts.Land;

public sealed record CreateLandRequest(
    [property: JsonPropertyName("crop_id")] ulong CropId,
    string Name,
    decimal Hectares,
    string State,
    string City,
    string? Neighborhood,
    string? Street,
    string? Number,
    decimal Latitude,
    decimal Longitude);

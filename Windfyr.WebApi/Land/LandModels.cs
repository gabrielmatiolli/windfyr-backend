using System.Text.Json.Serialization;

namespace Windfyr.WebApi.Land;

public sealed record LandCropResponse(ulong Id, string Type);

public sealed record LandResponse(
    ulong Id,
    string Name,
    decimal Hectares,
    string City,
    string State,
    LandCropResponse Crop,
    decimal Latitude,
    decimal Longitude);

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

public sealed record UpdateLandRequest(
    string? Name,
    decimal? Hectares);

namespace Windfyr.WebApi.Contracts.Land;

public sealed record LandResponse(
    ulong Id,
    string Name,
    decimal Hectares,
    string City,
    string State,
    LandCropResponse Crop,
    decimal Latitude,
    decimal Longitude,
    IReadOnlyList<LandForecastHistoryResponse> ForecastHistories,
    IReadOnlyList<LandIrrigationHistoryResponse> IrrigationHistories);

public sealed record LandCropResponse(ulong Id, string Type);

public sealed record LandForecastHistoryResponse(
    ulong Id,
    DateTime Date,
    decimal AirTemperature,
    decimal AirPressure,
    decimal AirHumidity,
    int RegionFires,
    int RegionFrosts,
    int RegionLightnings,
    decimal RegionFireRiskPercent);

public sealed record LandIrrigationHistoryResponse(
    ulong Id,
    DateTime Date,
    string Status);

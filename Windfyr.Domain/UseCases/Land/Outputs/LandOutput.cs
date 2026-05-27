namespace Windfyr.Domain.UseCases.Land.Outputs;

public sealed record LandOutput(
    ulong Id,
    string Name,
    decimal Hectares,
    string City,
    string State,
    LandCropOutput Crop,
    decimal Latitude,
    decimal Longitude,
    IReadOnlyList<LandForecastHistoryOutput> ForecastHistories,
    IReadOnlyList<LandIrrigationHistoryOutput> IrrigationHistories);

public sealed record LandCropOutput(ulong Id, string Type);

public sealed record LandForecastHistoryOutput(
    ulong Id,
    DateTime Date,
    decimal AirTemperature,
    decimal AirPressure,
    decimal AirHumidity,
    int RegionFires,
    int RegionFrosts,
    int RegionLightnings,
    decimal RegionFireRiskPercent);

public sealed record LandIrrigationHistoryOutput(
    ulong Id,
    DateTime Date,
    string Status);

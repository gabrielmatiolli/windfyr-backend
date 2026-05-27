namespace Windfyr.Domain.UseCases.Irrigation.Inputs;

public sealed record ApplyIrrigationRiskInput(
    Windfyr.Domain.Entities.Land Land,
    DateTime ReferenceDate,
    decimal FireRiskPercent);

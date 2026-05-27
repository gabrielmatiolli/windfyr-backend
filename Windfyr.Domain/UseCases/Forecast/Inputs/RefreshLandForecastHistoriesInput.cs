namespace Windfyr.Domain.UseCases.Forecast.Inputs;

public sealed record RefreshLandForecastHistoriesInput(IReadOnlyList<Windfyr.Domain.Entities.Land> Lands);

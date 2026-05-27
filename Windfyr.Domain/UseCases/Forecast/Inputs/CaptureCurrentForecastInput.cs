namespace Windfyr.Domain.UseCases.Forecast.Inputs;

public sealed record CaptureCurrentForecastInput(ulong UserId, ulong LandId);

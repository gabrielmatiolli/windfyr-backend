using Windfyr.Domain.UseCases.Forecast.Inputs;
using Windfyr.Domain.UseCases.Forecast.Interfaces;
using Windfyr.Domain.UseCases.Forecast.Outputs;
using Windfyr.Domain.UseCases.Land.Inputs;
using Windfyr.Domain.UseCases.Land.Interfaces;

namespace Windfyr.Domain.UseCases.Land.Implementations;

public sealed class GetLandForecastUseCase(ICaptureCurrentForecastUseCase forecastUseCase) : IGetLandForecastUseCase
{
    public Task<CurrentForecastOutput> ExecuteAsync(GetLandForecastInput input, CancellationToken ct)
    {
        return forecastUseCase.ExecuteAsync(
            new CaptureCurrentForecastInput(input.UserId, input.LandId),
            ct);
    }
}

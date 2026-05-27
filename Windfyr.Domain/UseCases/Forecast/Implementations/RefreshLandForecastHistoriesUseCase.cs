using Windfyr.Domain.UseCases.Forecast.Inputs;
using Windfyr.Domain.UseCases.Forecast.Interfaces;
using Windfyr.Domain.UseCases.Forecast.Outputs;

namespace Windfyr.Domain.UseCases.Forecast.Implementations;

public sealed class RefreshLandForecastHistoriesUseCase(
    CaptureCurrentForecastUseCase captureUseCase) : IRefreshLandForecastHistoriesUseCase
{
    public async Task<RefreshLandForecastHistoriesOutput> ExecuteAsync(RefreshLandForecastHistoriesInput input, CancellationToken ct)
    {
        foreach (var land in input.Lands)
        {
            await captureUseCase.CaptureCurrentForecastAsync(land, ct);
        }

        return new RefreshLandForecastHistoriesOutput();
    }
}

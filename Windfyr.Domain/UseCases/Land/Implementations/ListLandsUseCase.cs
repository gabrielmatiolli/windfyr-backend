using Windfyr.Domain.UseCases.Forecast.Inputs;
using Windfyr.Domain.UseCases.Forecast.Interfaces;
using Windfyr.Domain.UseCases.Land.Inputs;
using Windfyr.Domain.UseCases.Land.Interfaces;
using Windfyr.Domain.UseCases.Land.Outputs;
using Windfyr.Domain.Repositories.Interfaces;

namespace Windfyr.Domain.UseCases.Land.Implementations;

public sealed class ListLandsUseCase(
    ILandRepository landRepository,
    IRefreshLandForecastHistoriesUseCase refreshForecastUseCase) : IListLandsUseCase
{
    public async Task<ListLandsOutput> ExecuteAsync(ListLandsInput input, CancellationToken ct)
    {
        var lands = await landRepository.GetAllByUserAsync(input.UserId, ct);
        await refreshForecastUseCase.ExecuteAsync(new RefreshLandForecastHistoriesInput(lands), ct);

        var outputs = lands.Select(MapLandOutput).ToList();
        return new ListLandsOutput(outputs);
    }

    private static LandOutput MapLandOutput(Windfyr.Domain.Entities.Land land)
    {
        return new LandOutput(
            land.Id,
            land.Name,
            land.Hectares,
            land.City,
            land.State,
            new LandCropOutput(land.Crop.Id, land.Crop.Type),
            land.Latitude,
            land.Longitude,
            land.ForecastHistories
                .OrderByDescending(history => history.Date)
                .Take(1)
                .Select(history => new LandForecastHistoryOutput(
                    history.Id,
                    history.Date,
                    history.AirTemperature,
                    history.AirPressure,
                    history.AirHumidity,
                    history.RegionFires,
                    history.RegionFrosts,
                    history.RegionLightnings,
                    history.RegionFireRiskPercent))
                .ToList(),
            land.IrrigationHistories
                .OrderByDescending(history => history.Date)
                .Take(3)
                .Select(history => new LandIrrigationHistoryOutput(
                    history.Id,
                    history.Date,
                    history.Status))
                .ToList());
    }
}

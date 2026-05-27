using Windfyr.Domain.Repositories.Interfaces;
using Windfyr.Domain.UseCases.Land.Inputs;
using Windfyr.Domain.UseCases.Land.Interfaces;
using Windfyr.Domain.UseCases.Land.Outputs;

namespace Windfyr.Domain.UseCases.Land.Implementations;

public sealed class GetLandUseCase(ILandRepository landRepository) : IGetLandUseCase
{
    public async Task<GetLandOutput> ExecuteAsync(GetLandInput input, CancellationToken ct)
    {
        var land = await landRepository.GetByIdAsync(input.UserId, input.LandId, ct);
        if (land is null)
        {
            throw new KeyNotFoundException();
        }

        return new GetLandOutput(MapLandOutput(land));
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

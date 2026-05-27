using Windfyr.Domain.Repositories.Interfaces;
using Windfyr.Domain.UseCases.Land.Inputs;
using Windfyr.Domain.UseCases.Land.Interfaces;
using Windfyr.Domain.UseCases.Land.Outputs;

namespace Windfyr.Domain.UseCases.Land.Implementations;

public sealed class UpdateLandUseCase(ILandRepository landRepository) : IUpdateLandUseCase
{
    public async Task<UpdateLandOutput> ExecuteAsync(UpdateLandInput input, CancellationToken ct)
    {
        if (input.Name is null && input.Hectares is null)
        {
            throw new InvalidOperationException("Informe ao menos um campo para atualizar.");
        }

        if (input.Name is not null && string.IsNullOrWhiteSpace(input.Name))
        {
            throw new InvalidOperationException("Nome invalido.");
        }

        if (input.Hectares.HasValue && input.Hectares.Value <= 0)
        {
            throw new InvalidOperationException("Hectares deve ser maior que zero.");
        }

        var land = await landRepository.GetByIdAsync(input.UserId, input.LandId, ct);
        if (land is null)
        {
            throw new KeyNotFoundException();
        }

        if (input.Name is not null)
        {
            land.Name = input.Name.Trim();
        }

        if (input.Hectares.HasValue)
        {
            land.Hectares = input.Hectares.Value;
        }

        land.UpdatedAt = DateTime.UtcNow;

        await landRepository.UpdateAsync(land, ct);

        var updated = await landRepository.GetByIdAsync(input.UserId, land.Id, ct);
        if (updated is null)
        {
            throw new InvalidOperationException("Falha ao carregar terreno atualizado.");
        }

        return new UpdateLandOutput(MapLandOutput(updated));
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

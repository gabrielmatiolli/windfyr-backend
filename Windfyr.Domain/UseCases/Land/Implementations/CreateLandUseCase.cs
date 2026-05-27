using Windfyr.Domain.Entities;
using Windfyr.Domain.Repositories.Interfaces;
using Windfyr.Domain.UseCases.Land.Inputs;
using Windfyr.Domain.UseCases.Land.Interfaces;
using Windfyr.Domain.UseCases.Land.Outputs;

namespace Windfyr.Domain.UseCases.Land.Implementations;

public sealed class CreateLandUseCase(
    ILandRepository landRepository,
    ICropRepository cropRepository) : ICreateLandUseCase
{
    public async Task<CreateLandOutput> ExecuteAsync(CreateLandInput input, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(input.Name)
            || string.IsNullOrWhiteSpace(input.State)
            || string.IsNullOrWhiteSpace(input.City))
        {
            throw new InvalidOperationException("Nome, estado e cidade sao obrigatorios.");
        }

        if (input.Hectares <= 0)
        {
            throw new InvalidOperationException("Hectares deve ser maior que zero.");
        }

        var cropExists = await cropRepository.ExistsByIdAsync(input.CropId, ct);
        if (!cropExists)
        {
            throw new InvalidOperationException("Tipo de plantacao invalido.");
        }

        var land = new Windfyr.Domain.Entities.Land
        {
            UserId = input.UserId,
            CropId = input.CropId,
            Name = input.Name.Trim(),
            Hectares = input.Hectares,
            State = input.State.Trim(),
            City = input.City.Trim(),
            Neighborhood = input.Neighborhood?.Trim(),
            Street = input.Street?.Trim(),
            Number = input.Number?.Trim(),
            Latitude = input.Latitude,
            Longitude = input.Longitude,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await landRepository.AddAsync(land, ct);

        var created = await landRepository.GetByIdAsync(input.UserId, land.Id, ct);
        if (created is null)
        {
            throw new InvalidOperationException("Falha ao carregar terreno criado.");
        }

        return new CreateLandOutput(MapLandOutput(created));
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

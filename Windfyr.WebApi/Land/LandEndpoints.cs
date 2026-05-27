using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Windfyr.Repository.Context;
using Windfyr.WebApi.Forecast;

namespace Windfyr.WebApi.Land;

public static class LandEndpoints
{
    public static RouteGroupBuilder MapLandEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/lands")
            .WithTags("Lands")
            .RequireAuthorization();

        group.MapGet(string.Empty, GetLandsAsync)
            .WithName("GetLands")
            .WithSummary("Lista terrenos do usuario logado");

        group.MapGet("/{id}", GetLandByIdAsync)
            .WithName("GetLandById")
            .WithSummary("Detalha um terreno do usuario logado");

        group.MapGet("/{id}/forecast", GetLandForecastAsync)
            .WithName("GetLandForecast")
            .WithSummary("Busca previsao atual no OpenWeatherMap para um terreno do usuario logado");

        group.MapPost(string.Empty, CreateLandAsync)
            .WithName("CreateLand")
            .WithSummary("Cria terreno para usuario logado");

        group.MapPut("/{id}", UpdateLandAsync)
            .WithName("UpdateLand")
            .WithSummary("Atualiza terreno do usuario logado");

        group.MapDelete("/{id}", DeleteLandAsync)
            .WithName("DeleteLand")
            .WithSummary("Remove terreno do usuario logado");

        return group;
    }

    private static async Task<IResult> GetLandsAsync(
        ClaimsPrincipal principal,
        WindfyrDbContext dbContext,
        IForecastService forecastService,
        CancellationToken ct)
    {
        if (!TryGetUserId(principal, out var userId))
        {
            return Results.Unauthorized();
        }

        var lands = await dbContext.Lands
            .AsNoTracking()
            .Include(l => l.Crop)
            .Include(l => l.ForecastHistories)
            .Include(l => l.IrrigationHistories)
            .Where(l => l.UserId == userId)
            .OrderBy(l => l.Name)
            .ToListAsync(ct);

        await forecastService.RefreshLandForecastHistoriesAsync(lands, ct);

        return Results.Ok(lands.Select(MapLandResponse));
    }

    private static async Task<IResult> GetLandByIdAsync(
        ulong id,
        ClaimsPrincipal principal,
        WindfyrDbContext dbContext,
        CancellationToken ct)
    {
        if (!TryGetUserId(principal, out var userId))
        {
            return Results.Unauthorized();
        }

        var land = await dbContext.Lands
            .AsNoTracking()
            .Include(l => l.Crop)
            .Include(l => l.ForecastHistories)
            .Include(l => l.IrrigationHistories)
            .FirstOrDefaultAsync(l => l.Id == id && l.UserId == userId, ct);

        return land is null ? Results.NotFound() : Results.Ok(MapLandResponse(land));
    }

    private static async Task<IResult> CreateLandAsync(
        CreateLandRequest request,
        ClaimsPrincipal principal,
        WindfyrDbContext dbContext,
        CancellationToken ct)
    {
        if (!TryGetUserId(principal, out var userId))
        {
            return Results.Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(request.Name)
            || string.IsNullOrWhiteSpace(request.State)
            || string.IsNullOrWhiteSpace(request.City))
        {
            return Results.BadRequest(new { message = "Nome, estado e cidade sao obrigatorios." });
        }

        if (request.Hectares <= 0)
        {
            return Results.BadRequest(new { message = "Hectares deve ser maior que zero." });
        }

        var cropExists = await dbContext.Crops
            .AsNoTracking()
            .AnyAsync(c => c.Id == request.CropId, ct);

        if (!cropExists)
        {
            return Results.BadRequest(new { message = "Tipo de plantacao invalido." });
        }

        var land = new Windfyr.Domain.Entities.Land
        {
            UserId = userId,
            CropId = request.CropId,
            Name = request.Name.Trim(),
            Hectares = request.Hectares,
            State = request.State.Trim(),
            City = request.City.Trim(),
            Neighborhood = request.Neighborhood?.Trim(),
            Street = request.Street?.Trim(),
            Number = request.Number?.Trim(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        dbContext.Lands.Add(land);
        await dbContext.SaveChangesAsync(ct);

        var created = await dbContext.Lands
            .AsNoTracking()
            .Include(l => l.Crop)
            .Include(l => l.ForecastHistories)
            .Include(l => l.IrrigationHistories)
            .FirstAsync(l => l.Id == land.Id, ct);

        return Results.Created($"/api/lands/{land.Id}", MapLandResponse(created));
    }

    private static LandResponse MapLandResponse(Windfyr.Domain.Entities.Land land)
    {
        return new LandResponse(
            land.Id,
            land.Name,
            land.Hectares,
            land.City,
            land.State,
            new LandCropResponse(land.Crop.Id, land.Crop.Type),
            land.Latitude,
            land.Longitude,
            land.ForecastHistories
                .OrderByDescending(history => history.Date)
                .Take(1)
                .Select(history => new LandForecastHistoryResponse(
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
                .Take(1)
                .Select(history => new LandIrrigationHistoryResponse(
                    history.Id,
                    history.Date,
                    history.Status))
                .ToList());
    }

    private static async Task<IResult> GetLandForecastAsync(
        ulong id,
        ClaimsPrincipal principal,
        IForecastService forecastService,
        CancellationToken ct)
    {
        if (!TryGetUserId(principal, out var userId))
        {
            return Results.Unauthorized();
        }

        try
        {
            var response = await forecastService.CaptureCurrentForecastAsync(id, userId, ct);
            return Results.Ok(response);
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound();
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Falha ao consultar previsao externa",
                detail: ex.Message,
                statusCode: StatusCodes.Status502BadGateway);
        }
    }

    private static async Task<IResult> UpdateLandAsync(
        ulong id,
        UpdateLandRequest request,
        ClaimsPrincipal principal,
        WindfyrDbContext dbContext,
        CancellationToken ct)
    {
        if (!TryGetUserId(principal, out var userId))
        {
            return Results.Unauthorized();
        }

        if (request.Name is null && request.Hectares is null)
        {
            return Results.BadRequest(new { message = "Informe ao menos um campo para atualizar." });
        }

        if (request.Name is not null && string.IsNullOrWhiteSpace(request.Name))
        {
            return Results.BadRequest(new { message = "Nome invalido." });
        }

        if (request.Hectares.HasValue && request.Hectares.Value <= 0)
        {
            return Results.BadRequest(new { message = "Hectares deve ser maior que zero." });
        }

        var land = await dbContext.Lands
            .FirstOrDefaultAsync(l => l.Id == id && l.UserId == userId, ct);

        if (land is null)
        {
            return Results.NotFound();
        }

        if (request.Name is not null)
        {
            land.Name = request.Name.Trim();
        }

        if (request.Hectares.HasValue)
        {
            land.Hectares = request.Hectares.Value;
        }

        land.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(ct);

        var updated = await dbContext.Lands
            .AsNoTracking()
            .Include(l => l.Crop)
            .Include(l => l.ForecastHistories)
            .Include(l => l.IrrigationHistories)
            .FirstAsync(l => l.Id == land.Id, ct);

        return Results.Ok(MapLandResponse(updated));
    }

    private static async Task<IResult> DeleteLandAsync(
        ulong id,
        ClaimsPrincipal principal,
        WindfyrDbContext dbContext,
        CancellationToken ct)
    {
        if (!TryGetUserId(principal, out var userId))
        {
            return Results.Unauthorized();
        }

        var land = await dbContext.Lands
            .FirstOrDefaultAsync(l => l.Id == id && l.UserId == userId, ct);

        if (land is null)
        {
            return Results.NotFound();
        }

        dbContext.Lands.Remove(land);
        await dbContext.SaveChangesAsync(ct);

        return Results.NoContent();
    }

    private static bool TryGetUserId(ClaimsPrincipal principal, out ulong userId)
    {
        var userIdClaim = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

        userId = default;

        return !string.IsNullOrWhiteSpace(userIdClaim)
            && ulong.TryParse(userIdClaim, out userId);
    }
}

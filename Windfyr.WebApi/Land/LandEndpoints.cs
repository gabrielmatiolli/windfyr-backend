using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Windfyr.Repository.Context;

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

        group.MapGet("/{id:ulong}", GetLandByIdAsync)
            .WithName("GetLandById")
            .WithSummary("Detalha um terreno do usuario logado");

        group.MapPost(string.Empty, CreateLandAsync)
            .WithName("CreateLand")
            .WithSummary("Cria terreno para usuario logado");

        group.MapPut("/{id:ulong}", UpdateLandAsync)
            .WithName("UpdateLand")
            .WithSummary("Atualiza terreno do usuario logado");

        group.MapDelete("/{id:ulong}", DeleteLandAsync)
            .WithName("DeleteLand")
            .WithSummary("Remove terreno do usuario logado");

        return group;
    }

    private static async Task<IResult> GetLandsAsync(
        ClaimsPrincipal principal,
        WindfyrDbContext dbContext,
        CancellationToken ct)
    {
        if (!TryGetUserId(principal, out var userId))
        {
            return Results.Unauthorized();
        }

        var lands = await dbContext.Lands
            .AsNoTracking()
            .Where(l => l.UserId == userId)
            .OrderBy(l => l.Name)
            .Select(l => new LandResponse(
                l.Id,
                l.Name,
                l.Hectares,
                l.City,
                l.State,
                new LandCropResponse(l.Crop.Id, l.Crop.Type),
                l.Latitude,
                l.Longitude))
            .ToListAsync(ct);

        return Results.Ok(lands);
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
            .Where(l => l.Id == id && l.UserId == userId)
            .Select(l => new LandResponse(
                l.Id,
                l.Name,
                l.Hectares,
                l.City,
                l.State,
                new LandCropResponse(l.Crop.Id, l.Crop.Type),
                l.Latitude,
                l.Longitude))
            .FirstOrDefaultAsync(ct);

        return land is null ? Results.NotFound() : Results.Ok(land);
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
            .Where(l => l.Id == land.Id)
            .Select(l => new LandResponse(
                l.Id,
                l.Name,
                l.Hectares,
                l.City,
                l.State,
                new LandCropResponse(l.Crop.Id, l.Crop.Type),
                l.Latitude,
                l.Longitude))
            .FirstAsync(ct);

        return Results.Created($"/api/lands/{land.Id}", created);
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
            .Where(l => l.Id == land.Id)
            .Select(l => new LandResponse(
                l.Id,
                l.Name,
                l.Hectares,
                l.City,
                l.State,
                new LandCropResponse(l.Crop.Id, l.Crop.Type),
                l.Latitude,
                l.Longitude))
            .FirstAsync(ct);

        return Results.Ok(updated);
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

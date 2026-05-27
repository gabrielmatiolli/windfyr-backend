using Microsoft.EntityFrameworkCore;
using Windfyr.Repository.Context;

namespace Windfyr.WebApi.Crop;

public static class CropEndpoints
{
    public static RouteGroupBuilder MapCropEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/crops").WithTags("Crops");

        group.MapGet(string.Empty, GetCropsAsync)
            .WithName("GetCrops")
            .WithSummary("Lista tipos de plantacao");

        return group;
    }

    private static async Task<IResult> GetCropsAsync(
        WindfyrDbContext dbContext,
        CancellationToken ct)
    {
        var crops = await dbContext.Crops
            .AsNoTracking()
            .OrderBy(c => c.Type)
            .Select(c => new CropResponse(c.Id, c.Type))
            .ToListAsync(ct);

        return Results.Ok(crops);
    }
}
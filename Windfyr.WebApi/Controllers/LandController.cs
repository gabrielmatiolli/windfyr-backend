using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Windfyr.Domain.UseCases.Forecast.Outputs;
using Windfyr.Domain.UseCases.Land.Inputs;
using Windfyr.Domain.UseCases.Land.Interfaces;
using Windfyr.WebApi.Contracts.Forecast;
using Windfyr.WebApi.Contracts.Land;

namespace Windfyr.WebApi.Controllers;

[ApiController]
[Route("api/lands")]
[Authorize]
public sealed class LandController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetLandsAsync(
        [FromServices] IListLandsUseCase useCase,
        CancellationToken ct)
    {
        if (!TryGetUserId(User, out var userId))
        {
            return Unauthorized();
        }

        var output = await useCase.ExecuteAsync(new ListLandsInput(userId), ct);
        var response = output.Lands.Select(MapLandResponse);

        return Ok(response);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetLandByIdAsync(
        [FromRoute] ulong id,
        [FromServices] IGetLandUseCase useCase,
        CancellationToken ct)
    {
        if (!TryGetUserId(User, out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var output = await useCase.ExecuteAsync(new GetLandInput(userId, id), ct);
            return Ok(MapLandResponse(output.Land));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("{id:long}/forecast")]
    public async Task<IActionResult> GetLandForecastAsync(
        [FromRoute] ulong id,
        [FromServices] IGetLandForecastUseCase useCase,
        CancellationToken ct)
    {
        if (!TryGetUserId(User, out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var output = await useCase.ExecuteAsync(new GetLandForecastInput(userId, id), ct);
            return Ok(MapForecastResponse(output));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return Problem(
                title: "Falha ao consultar previsao externa",
                detail: ex.Message,
                statusCode: StatusCodes.Status502BadGateway);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateLandAsync(
        [FromBody] CreateLandRequest request,
        [FromServices] ICreateLandUseCase useCase,
        CancellationToken ct)
    {
        if (!TryGetUserId(User, out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var output = await useCase.ExecuteAsync(
                new CreateLandInput(
                    userId,
                    request.CropId,
                    request.Name,
                    request.Hectares,
                    request.State,
                    request.City,
                    request.Neighborhood,
                    request.Street,
                    request.Number,
                    request.Latitude,
                    request.Longitude),
                ct);

            return Created($"/api/lands/{output.Land.Id}", MapLandResponse(output.Land));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdateLandAsync(
        [FromRoute] ulong id,
        [FromBody] UpdateLandRequest request,
        [FromServices] IUpdateLandUseCase useCase,
        CancellationToken ct)
    {
        if (!TryGetUserId(User, out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var output = await useCase.ExecuteAsync(
                new UpdateLandInput(userId, id, request.Name, request.Hectares),
                ct);

            return Ok(MapLandResponse(output.Land));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteLandAsync(
        [FromRoute] ulong id,
        [FromServices] IDeleteLandUseCase useCase,
        CancellationToken ct)
    {
        if (!TryGetUserId(User, out var userId))
        {
            return Unauthorized();
        }

        try
        {
            await useCase.ExecuteAsync(new DeleteLandInput(userId, id), ct);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    private static LandResponse MapLandResponse(Windfyr.Domain.UseCases.Land.Outputs.LandOutput land)
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
                .Select(history => new LandIrrigationHistoryResponse(
                    history.Id,
                    history.Date,
                    history.Status))
                .ToList());
    }

    private static CurrentForecastResponse MapForecastResponse(CurrentForecastOutput output)
    {
        return new CurrentForecastResponse(
            output.LandId,
            output.LandName,
            output.Latitude,
            output.Longitude,
            new CurrentForecastDataResponse(
                output.Forecast.TemperatureCelsius,
                output.Forecast.PressureHpa,
                output.Forecast.HumidityPercent,
                output.Forecast.Description,
                output.Forecast.Icon,
                output.Forecast.DataTimeUtc,
                output.Forecast.CityName,
                output.Forecast.RegionFireRiskPercent));
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

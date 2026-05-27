using Microsoft.AspNetCore.Mvc;
using Windfyr.Domain.UseCases.Crop.Inputs;
using Windfyr.Domain.UseCases.Crop.Interfaces;
using Windfyr.WebApi.Contracts.Crop;

namespace Windfyr.WebApi.Controllers;

[ApiController]
[Route("api/crops")]
public sealed class CropController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCropsAsync(
        [FromServices] IListCropsUseCase useCase,
        CancellationToken ct)
    {
        var output = await useCase.ExecuteAsync(new ListCropsInput(), ct);
        var response = output.Crops
            .Select(crop => new CropResponse(crop.Id, crop.Type))
            .ToList();

        return Ok(response);
    }
}

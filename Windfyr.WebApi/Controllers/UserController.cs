using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Windfyr.Domain.UseCases.User.Inputs;
using Windfyr.Domain.UseCases.User.Interfaces;
using Windfyr.WebApi.Contracts.User;

namespace Windfyr.WebApi.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public sealed class UserController : ControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> GetProfileAsync(
        [FromServices] IGetCurrentUserUseCase useCase,
        CancellationToken ct)
    {
        if (!TryGetUserId(User, out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var output = await useCase.ExecuteAsync(new GetCurrentUserInput(userId), ct);
            return Ok(new
            {
                id = output.Id,
                name = output.Name,
                email = output.Email
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfileAsync(
        [FromBody] UpdateCurrentUserRequest request,
        [FromServices] IUpdateCurrentUserUseCase useCase,
        CancellationToken ct)
    {
        if (!TryGetUserId(User, out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var output = await useCase.ExecuteAsync(
                new UpdateCurrentUserInput(
                    userId,
                    request.Name,
                    request.Email,
                    request.Password,
                    request.PasswordConfirmation),
                ct);

            return Ok(new
            {
                message = output.Message,
                user = new
                {
                    id = output.User.Id,
                    name = output.User.Name,
                    email = output.User.Email
                }
            });
        }
        catch (InvalidOperationException ex)
        {
            if (string.Equals(ex.Message, "Email ja cadastrado.", StringComparison.Ordinal))
            {
                return Conflict(new { message = ex.Message });
            }

            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
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

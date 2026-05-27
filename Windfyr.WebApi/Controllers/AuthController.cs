using Microsoft.AspNetCore.Mvc;
using Windfyr.Domain.UseCases.Auth.Inputs;
using Windfyr.Domain.UseCases.Auth.Interfaces;
using Windfyr.WebApi.Contracts.Auth;

namespace Windfyr.WebApi.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync(
        [FromBody] RegisterRequest request,
        [FromServices] IRegisterUserUseCase useCase,
        CancellationToken ct)
    {
        try
        {
            var output = await useCase.ExecuteAsync(
                new RegisterUserInput(
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
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync(
        [FromBody] LoginRequest request,
        [FromServices] ILoginUserUseCase useCase,
        CancellationToken ct)
    {
        try
        {
            var output = await useCase.ExecuteAsync(
                new LoginUserInput(request.Email, request.Password),
                ct);

            return Ok(new
            {
                token = output.Token,
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
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
    }
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Windfyr.Repository.Context;

namespace Windfyr.WebApi.User;

public static class UserEndpoints
{
    public static RouteGroupBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/user").WithTags("User");

        group.MapGet("/profile", GetProfileAsync)
            .WithName("UserProfile")
            .WithSummary("Perfil do usuario logado")
            .RequireAuthorization();

        group.MapPut("/profile", UpdateProfileAsync)
            .WithName("UpdateUserProfile")
            .WithSummary("Atualiza perfil do usuario logado")
            .RequireAuthorization();

        return group;
    }

    private static async Task<IResult> GetProfileAsync(
        ClaimsPrincipal principal,
        WindfyrDbContext dbContext,
        CancellationToken ct)
    {
        var userIdClaim = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userIdClaim) || !ulong.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        var user = await dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                id = u.Id,
                name = u.Name,
                email = u.Email
            })
            .FirstOrDefaultAsync(ct);

        return user is null ? Results.Unauthorized() : Results.Ok(user);
    }

    private static async Task<IResult> UpdateProfileAsync(
        UpdateUserProfileRequest request,
        ClaimsPrincipal principal,
        WindfyrDbContext dbContext,
        CancellationToken ct)
    {
        var userIdClaim = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userIdClaim) || !ulong.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email))
        {
            return Results.BadRequest(new { message = "Nome e email sao obrigatorios." });
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user is null)
        {
            return Results.Unauthorized();
        }

        var emailAlreadyExists = await dbContext.Users
            .AsNoTracking()
            .AnyAsync(u => u.Email == normalizedEmail && u.Id != userId, ct);

        if (emailAlreadyExists)
        {
            return Results.Conflict(new { message = "Email ja cadastrado." });
        }

        var shouldUpdatePassword =
            !string.IsNullOrWhiteSpace(request.Password)
            || !string.IsNullOrWhiteSpace(request.PasswordConfirmation);

        if (shouldUpdatePassword)
        {
            if (string.IsNullOrWhiteSpace(request.Password)
                || string.IsNullOrWhiteSpace(request.PasswordConfirmation))
            {
                return Results.BadRequest(new { message = "Para alterar a senha, informe senha e confirmacao." });
            }

            if (request.Password.Length < 6)
            {
                return Results.BadRequest(new { message = "A senha deve ter no minimo 6 caracteres." });
            }

            if (!string.Equals(request.Password, request.PasswordConfirmation, StringComparison.Ordinal))
            {
                return Results.BadRequest(new { message = "A confirmacao de senha nao confere." });
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
        }

        user.Name = request.Name.Trim();
        user.Email = normalizedEmail;
        user.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(ct);

        return Results.Ok(new
        {
            message = "Perfil atualizado com sucesso.",
            user = new
            {
                id = user.Id,
                name = user.Name,
                email = user.Email
            }
        });
    }

}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Windfyr.Domain.Entities;
using Windfyr.Repository.Context;

namespace Windfyr.WebApi.Auth;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", RegisterAsync)
            .WithName("AuthRegister")
            .WithSummary("Cadastro de usuario");

        group.MapPost("/login", LoginAsync)
            .WithName("AuthLogin")
            .WithSummary("Login do usuario");

        return group;
    }

    private static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        WindfyrDbContext dbContext,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name)
            || string.IsNullOrWhiteSpace(request.Email)
            || string.IsNullOrWhiteSpace(request.Password)
            || string.IsNullOrWhiteSpace(request.PasswordConfirmation))
        {
            return Results.BadRequest(new { message = "Todos os campos sao obrigatorios." });
        }

        if (request.Password.Length < 6)
        {
            return Results.BadRequest(new { message = "A senha deve ter no minimo 6 caracteres." });
        }

        if (!string.Equals(request.Password, request.PasswordConfirmation, StringComparison.Ordinal))
        {
            return Results.BadRequest(new { message = "A confirmacao de senha nao confere." });
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var emailAlreadyExists = await dbContext.Users
            .AsNoTracking()
            .AnyAsync(u => u.Email == normalizedEmail, ct);

        if (emailAlreadyExists)
        {
            return Results.Conflict(new { message = "Email ja cadastrado." });
        }

        var user = new Windfyr.Domain.Entities.User
        {
            Name = request.Name.Trim(),
            Email = normalizedEmail,
            Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(ct);

        return Results.Ok(new
        {
            message = "Usuario criado com sucesso",
            user = new
            {
                id = user.Id,
                name = user.Name,
                email = user.Email
            }
        });
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        WindfyrDbContext dbContext,
        AuthSettings authSettings,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Results.BadRequest(new { message = "Email e senha sao obrigatorios." });
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, ct);

        if (user is null)
        {
            return Results.Unauthorized();
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
        {
            return Results.Unauthorized();
        }

        var token = GenerateToken(user, authSettings);

        return Results.Ok(new
        {
            token,
            user = new
            {
                id = user.Id,
                name = user.Name,
                email = user.Email
            }
        });
    }

    private static string GenerateToken(Windfyr.Domain.Entities.User user, AuthSettings authSettings)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authSettings.Key));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Name, user.Name),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(authSettings.ExpiresInMinutes),
            Issuer = authSettings.Issuer,
            Audience = authSettings.Audience,
            SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

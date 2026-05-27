using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Windfyr.Domain.Repositories.Interfaces;
using Windfyr.Domain.UseCases.Auth.Inputs;
using Windfyr.Domain.UseCases.Auth.Interfaces;
using Windfyr.Domain.UseCases.Auth.Outputs;

namespace Windfyr.Domain.UseCases.Auth.Implementations;

public sealed class LoginUserUseCase(
    IUserRepository userRepository,
    AuthTokenSettings tokenSettings) : ILoginUserUseCase
{
    public async Task<LoginUserOutput> ExecuteAsync(LoginUserInput input, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(input.Email) || string.IsNullOrWhiteSpace(input.Password))
        {
            throw new InvalidOperationException("Email e senha sao obrigatorios.");
        }

        var normalizedEmail = input.Email.Trim().ToLowerInvariant();
        var user = await userRepository.GetByEmailAsync(normalizedEmail, ct);

        if (user is null || !BCrypt.Net.BCrypt.Verify(input.Password, user.Password))
        {
            throw new UnauthorizedAccessException();
        }

        var token = GenerateToken(user, tokenSettings);

        return new LoginUserOutput(
            Token: token,
            User: new LoggedUserOutput(user.Id, user.Name, user.Email));
    }

    private static string GenerateToken(Windfyr.Domain.Entities.User user, AuthTokenSettings settings)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key));

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
            Expires = DateTime.UtcNow.AddMinutes(settings.ExpiresInMinutes),
            Issuer = settings.Issuer,
            Audience = settings.Audience,
            SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

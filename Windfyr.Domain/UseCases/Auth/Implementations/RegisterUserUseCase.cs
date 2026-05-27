using Windfyr.Domain.Entities;
using Windfyr.Domain.Repositories.Interfaces;
using Windfyr.Domain.UseCases.Auth.Inputs;
using Windfyr.Domain.UseCases.Auth.Interfaces;
using Windfyr.Domain.UseCases.Auth.Outputs;

namespace Windfyr.Domain.UseCases.Auth.Implementations;

public sealed class RegisterUserUseCase(IUserRepository userRepository) : IRegisterUserUseCase
{
    public async Task<RegisterUserOutput> ExecuteAsync(RegisterUserInput input, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(input.Name)
            || string.IsNullOrWhiteSpace(input.Email)
            || string.IsNullOrWhiteSpace(input.Password)
            || string.IsNullOrWhiteSpace(input.PasswordConfirmation))
        {
            throw new InvalidOperationException("Todos os campos sao obrigatorios.");
        }

        if (input.Password.Length < 6)
        {
            throw new InvalidOperationException("A senha deve ter no minimo 6 caracteres.");
        }

        if (!string.Equals(input.Password, input.PasswordConfirmation, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("A confirmacao de senha nao confere.");
        }

        var normalizedEmail = input.Email.Trim().ToLowerInvariant();
        var emailAlreadyExists = await userRepository.ExistsByEmailAsync(normalizedEmail, ct);
        if (emailAlreadyExists)
        {
            throw new InvalidOperationException("Email ja cadastrado.");
        }

        var user = new Windfyr.Domain.Entities.User
        {
            Name = input.Name.Trim(),
            Email = normalizedEmail,
            Password = BCrypt.Net.BCrypt.HashPassword(input.Password),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await userRepository.AddAsync(user, ct);

        return new RegisterUserOutput(
            Message: "Usuario criado com sucesso",
            User: new RegisteredUserOutput(user.Id, user.Name, user.Email));
    }
}

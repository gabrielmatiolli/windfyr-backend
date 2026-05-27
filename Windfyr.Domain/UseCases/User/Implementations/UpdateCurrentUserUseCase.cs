using Windfyr.Domain.Repositories.Interfaces;
using Windfyr.Domain.UseCases.User.Inputs;
using Windfyr.Domain.UseCases.User.Interfaces;
using Windfyr.Domain.UseCases.User.Outputs;

namespace Windfyr.Domain.UseCases.User.Implementations;

public sealed class UpdateCurrentUserUseCase(IUserRepository userRepository) : IUpdateCurrentUserUseCase
{
    public async Task<UpdateCurrentUserOutput> ExecuteAsync(UpdateCurrentUserInput input, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(input.Name) || string.IsNullOrWhiteSpace(input.Email))
        {
            throw new InvalidOperationException("Nome e email sao obrigatorios.");
        }

        var normalizedEmail = input.Email.Trim().ToLowerInvariant();
        var user = await userRepository.GetByIdAsync(input.UserId, ct);

        if (user is null)
        {
            throw new UnauthorizedAccessException();
        }

        var emailAlreadyExists = await userRepository.ExistsByEmailAsync(normalizedEmail, ct);
        if (emailAlreadyExists && !string.Equals(user.Email, normalizedEmail, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Email ja cadastrado.");
        }

        var shouldUpdatePassword =
            !string.IsNullOrWhiteSpace(input.Password)
            || !string.IsNullOrWhiteSpace(input.PasswordConfirmation);

        if (shouldUpdatePassword)
        {
            if (string.IsNullOrWhiteSpace(input.Password)
                || string.IsNullOrWhiteSpace(input.PasswordConfirmation))
            {
                throw new InvalidOperationException("Para alterar a senha, informe senha e confirmacao.");
            }

            if (input.Password.Length < 6)
            {
                throw new InvalidOperationException("A senha deve ter no minimo 6 caracteres.");
            }

            if (!string.Equals(input.Password, input.PasswordConfirmation, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("A confirmacao de senha nao confere.");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(input.Password);
        }

        user.Name = input.Name.Trim();
        user.Email = normalizedEmail;
        user.UpdatedAt = DateTime.UtcNow;

        await userRepository.UpdateAsync(user, ct);

        return new UpdateCurrentUserOutput(
            Message: "Perfil atualizado com sucesso.",
            User: new UpdatedUserOutput(user.Id, user.Name, user.Email));
    }
}

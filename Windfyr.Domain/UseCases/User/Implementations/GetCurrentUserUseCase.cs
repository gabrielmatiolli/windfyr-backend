using Windfyr.Domain.Repositories.Interfaces;
using Windfyr.Domain.UseCases.User.Inputs;
using Windfyr.Domain.UseCases.User.Interfaces;
using Windfyr.Domain.UseCases.User.Outputs;

namespace Windfyr.Domain.UseCases.User.Implementations;

public sealed class GetCurrentUserUseCase(IUserRepository userRepository) : IGetCurrentUserUseCase
{
    public async Task<GetCurrentUserOutput> ExecuteAsync(GetCurrentUserInput input, CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(input.UserId, ct);
        if (user is null)
        {
            throw new UnauthorizedAccessException();
        }

        return new GetCurrentUserOutput(user.Id, user.Name, user.Email);
    }
}

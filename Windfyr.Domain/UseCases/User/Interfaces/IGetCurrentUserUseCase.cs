using Windfyr.Domain.Common;
using Windfyr.Domain.UseCases.User.Inputs;
using Windfyr.Domain.UseCases.User.Outputs;

namespace Windfyr.Domain.UseCases.User.Interfaces;

public interface IGetCurrentUserUseCase : IUseCase<GetCurrentUserInput, GetCurrentUserOutput>;

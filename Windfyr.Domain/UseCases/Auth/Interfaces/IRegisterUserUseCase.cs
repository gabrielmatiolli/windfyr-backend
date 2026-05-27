using Windfyr.Domain.Common;
using Windfyr.Domain.UseCases.Auth.Inputs;
using Windfyr.Domain.UseCases.Auth.Outputs;

namespace Windfyr.Domain.UseCases.Auth.Interfaces;

public interface IRegisterUserUseCase : IUseCase<RegisterUserInput, RegisterUserOutput>;

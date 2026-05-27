using Windfyr.Domain.Common;
using Windfyr.Domain.UseCases.Land.Inputs;
using Windfyr.Domain.UseCases.Land.Outputs;

namespace Windfyr.Domain.UseCases.Land.Interfaces;

public interface IGetLandUseCase : IUseCase<GetLandInput, GetLandOutput>;

using Windfyr.Domain.Common;
using Windfyr.Domain.UseCases.Crop.Inputs;
using Windfyr.Domain.UseCases.Crop.Outputs;

namespace Windfyr.Domain.UseCases.Crop.Interfaces;

public interface IListCropsUseCase : IUseCase<ListCropsInput, ListCropsOutput>;

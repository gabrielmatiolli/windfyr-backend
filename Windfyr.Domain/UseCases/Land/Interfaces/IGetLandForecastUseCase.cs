using Windfyr.Domain.Common;
using Windfyr.Domain.UseCases.Land.Inputs;
using Windfyr.Domain.UseCases.Forecast.Outputs;

namespace Windfyr.Domain.UseCases.Land.Interfaces;

public interface IGetLandForecastUseCase : IUseCase<GetLandForecastInput, CurrentForecastOutput>;

using Windfyr.Domain.Common;
using Windfyr.Domain.UseCases.Forecast.Inputs;
using Windfyr.Domain.UseCases.Forecast.Outputs;

namespace Windfyr.Domain.UseCases.Forecast.Interfaces;

public interface IRefreshLandForecastHistoriesUseCase : IUseCase<RefreshLandForecastHistoriesInput, RefreshLandForecastHistoriesOutput>;

using Windfyr.Domain.Common;
using Windfyr.Domain.UseCases.Irrigation.Inputs;
using Windfyr.Domain.UseCases.Irrigation.Outputs;

namespace Windfyr.Domain.UseCases.Irrigation.Interfaces;

public interface IApplyIrrigationRiskUseCase : IUseCase<ApplyIrrigationRiskInput, ApplyIrrigationRiskOutput>;

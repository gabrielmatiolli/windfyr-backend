using Windfyr.Repository.Context;

namespace Windfyr.WebApi.Irrigation;

public interface IIrrigationService
{
    void ApplyRiskStatusTransition(
        Windfyr.Domain.Entities.Land land,
        DateTime referenceDate,
        decimal fireRiskPercent);
}

public sealed class IrrigationService(WindfyrDbContext dbContext) : IIrrigationService
{
    private const decimal IrrigationActivationRiskThreshold = 80m;
    private const string IrrigationActivatedStatus = "ativado";
    private const string IrrigationDeactivatedStatus = "desativado";

    public void ApplyRiskStatusTransition(
        Windfyr.Domain.Entities.Land land,
        DateTime referenceDate,
        decimal fireRiskPercent)
    {
        var desiredStatus = fireRiskPercent > IrrigationActivationRiskThreshold
            ? IrrigationActivatedStatus
            : IrrigationDeactivatedStatus;

        var lastStatus = land.IrrigationHistories
            .OrderByDescending(history => history.Date)
            .Select(history => history.Status)
            .FirstOrDefault();

        if (string.Equals(lastStatus, desiredStatus, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var irrigationHistory = new Windfyr.Domain.Entities.IrrigationHistory
        {
            LandId = land.Id,
            Date = referenceDate,
            Status = desiredStatus,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        dbContext.IrrigationHistories.Add(irrigationHistory);
        land.IrrigationHistories.Add(irrigationHistory);
    }
}

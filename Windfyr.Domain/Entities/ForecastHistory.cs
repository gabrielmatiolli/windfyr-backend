namespace Windfyr.Domain.Entities;

public class ForecastHistory
{
    public ulong Id { get; set; }
    public ulong LandId { get; set; }
    public DateTime Date { get; set; }
    public decimal AirTemperature { get; set; }
    public decimal AirPressure { get; set; }
    public decimal AirHumidity { get; set; }
    public int RegionFires { get; set; }
    public int RegionFrosts { get; set; }
    public int RegionLightnings { get; set; }
    public decimal RegionFireRiskPercent { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Land Land { get; set; } = null!;
}

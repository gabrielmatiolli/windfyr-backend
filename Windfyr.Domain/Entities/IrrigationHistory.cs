namespace Windfyr.Domain.Entities;

public class IrrigationHistory
{
    public ulong Id { get; set; }
    public ulong LandId { get; set; }
    public DateTime Date { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Land Land { get; set; } = null!;
}

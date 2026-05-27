namespace Windfyr.Domain.Entities;

public class Land
{
    public ulong Id { get; set; }
    public ulong UserId { get; set; }
    public ulong CropId { get; set; }
    public decimal Hectares { get; set; }
    public string Name { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string? Neighborhood { get; set; }
    public string City { get; set; } = string.Empty;
    public string? Street { get; set; }
    public string? Number { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public Crop Crop { get; set; } = null!;
    public ICollection<ForecastHistory> ForecastHistories { get; set; } = new List<ForecastHistory>();
    public ICollection<IrrigationHistory> IrrigationHistories { get; set; } = new List<IrrigationHistory>();
}

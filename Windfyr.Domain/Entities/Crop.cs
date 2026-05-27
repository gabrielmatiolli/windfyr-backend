namespace Windfyr.Domain.Entities;

public class Crop
{
    public ulong Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Land> Lands { get; set; } = new List<Land>();
}

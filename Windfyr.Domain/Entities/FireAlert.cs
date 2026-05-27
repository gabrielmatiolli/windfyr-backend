namespace Windfyr.Domain.Entities
{
    public class FireAlert
    {
        public Guid Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public double Intensity { get; set; }
        public DateTime DetectedAt { get; set; }
    }
}

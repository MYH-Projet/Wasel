namespace Wasel.Api.Modules.Tracking.DTOs;

public class TrackingPointResponseDto
{
    public Guid? Id { get; set; }
    public bool Persisted { get; set; }
    
    public Guid DriverId { get; set; }
    public Guid? DeliveryId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? Heading { get; set; }
    public double? SpeedKmh { get; set; }
    public double? AccuracyMeters { get; set; }
    public DateTime RecordedAt { get; set; }
}

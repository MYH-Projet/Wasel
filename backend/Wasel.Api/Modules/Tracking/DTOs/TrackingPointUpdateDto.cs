using System.ComponentModel.DataAnnotations;

namespace Wasel.Api.Modules.Tracking.DTOs;

public class TrackingPointUpdateDto
{
    public Guid? DeliveryId { get; set; }

    [Range(-90.0, 90.0)]
    public double Latitude { get; set; }

    [Range(-180.0, 180.0)]
    public double Longitude { get; set; }

    public double? Heading { get; set; }

    [Range(0, double.MaxValue)]
    public double? SpeedKmh { get; set; }

    [Range(0, double.MaxValue)]
    public double? AccuracyMeters { get; set; }

    public DateTime? RecordedAt { get; set; }
}

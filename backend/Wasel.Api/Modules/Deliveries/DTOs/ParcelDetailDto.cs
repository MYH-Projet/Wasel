namespace Wasel.Api.Modules.Deliveries.DTOs;

public class ParcelDetailDto
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public double Weight { get; set; }
    public double Volume { get; set; }
    public bool IsFragile { get; set; }
    public string? ImageObjectKey { get; set; }
    public double EstimatedDistanceKm { get; set; }
    public double EstimatedDurationMin { get; set; }
}
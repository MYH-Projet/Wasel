namespace Wasel.Api.Modules.Deliveries.DTOs;

public class DeliveryEstimateResponseDto
{
    public decimal EstimatedPrice { get; set; }
    public string Currency { get; set; } = "MAD";
    public double DistanceKm { get; set; }
    public DeliveryEstimateBreakdownDto Breakdown { get; set; } = new();
}

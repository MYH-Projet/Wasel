namespace Wasel.Api.Modules.Deliveries.DTOs;

public class DeliveryEstimateBreakdownDto
{
    public decimal BaseFee { get; set; }
    public decimal DistanceFee { get; set; }
    public decimal WeightFee { get; set; }
    public decimal FragileFee { get; set; }
}

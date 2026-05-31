namespace Wasel.Api.Modules.Deliveries.DTOs;
using Wasel.Api.Modules.Deliveries.Enums;

public class UpdateDeliveryStatusRequestDto
{
    public DeliveryStatus NewStatus { get; set; }
    public string? Note { get; set; }
}
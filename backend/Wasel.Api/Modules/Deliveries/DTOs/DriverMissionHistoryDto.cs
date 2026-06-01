namespace Wasel.Api.Modules.Deliveries.DTOs;

public class DriverMissionHistoryDto
{
    public Guid DeliveryId { get; set; }

    public DateTime Date { get; set; }

    public string PickupAddress { get; set; } = string.Empty;

    public string DropoffAddress { get; set; } = string.Empty;

    public string FinalStatus { get; set; } = string.Empty;

    public decimal AmountEarned { get; set; }
}
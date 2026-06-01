using Wasel.Api.Modules.Payments.Enums;

namespace Wasel.Api.Modules.Payments.DTOs;

public class PaymentDetailsResponseDto
{
    public Guid PaymentId { get; set; }
    public Guid DeliveryId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    public string TransactionReference { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

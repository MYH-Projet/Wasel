using Wasel.Api.Modules.Payments.Enums;

namespace Wasel.Api.Modules.Payments.DTOs;

public class InitiatePaymentResponseDto
{
    public Guid PaymentId { get; set; }
    public PaymentStatus Status { get; set; }
    public string? PaymentUrl { get; set; } // for CARD
}

using System.ComponentModel.DataAnnotations;

namespace Wasel.Api.Modules.Payments.DTOs;

public class InitiatePaymentRequestDto
{
    [Required]
    public Guid DeliveryId { get; set; }

    [Required]
    public string PaymentMethod { get; set; } = string.Empty; // CASH, CARD, WALLET
}

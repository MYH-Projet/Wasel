namespace Wasel.Api.Modules.Deliveries.DTOs;

public class PaymentDetailDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string? ProviderName { get; set; }
    public string? TransactionReference { get; set; }
    public decimal? RefundedAt { get; set; }
}
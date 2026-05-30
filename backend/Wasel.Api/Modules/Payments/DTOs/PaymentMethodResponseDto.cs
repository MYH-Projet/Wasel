namespace Wasel.Api.Modules.Payments.DTOs;

public class PaymentMethodResponseDto
{
    public Guid Id { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string CardBrand { get; set; } = string.Empty;
    public string CardLast4 { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
}

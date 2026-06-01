using System.ComponentModel.DataAnnotations;

namespace Wasel.Api.Modules.Payments.DTOs;

public class CreatePaymentMethodRequestDto
{
    [Required]
    public string ProviderName { get; set; } = string.Empty;

    [Required]
    public string ProviderCustomerId { get; set; } = string.Empty;

    [Required]
    public string ProviderPaymentMethodId { get; set; } = string.Empty;

    [Required]
    public string CardBrand { get; set; } = string.Empty;

    [Required]
    public string CardLast4 { get; set; } = string.Empty;

    public bool IsDefault { get; set; } = false;
}

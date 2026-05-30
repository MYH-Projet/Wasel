using System.ComponentModel.DataAnnotations;

namespace Wasel.Api.Modules.Wallets.DTOs;

public class WithdrawRequestDto
{
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    [Required]
    public string Currency { get; set; } = string.Empty;
}

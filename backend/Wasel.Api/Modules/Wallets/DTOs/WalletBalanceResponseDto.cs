using Wasel.Api.Modules.Wallets.Enums;

namespace Wasel.Api.Modules.Wallets.DTOs;

public class WalletBalanceResponseDto
{
    public Guid WalletId { get; set; }
    public decimal Balance { get; set; }
    public string Currency { get; set; } = string.Empty;
    public WalletStatus Status { get; set; }
    public decimal? MonthlyEarnings { get; set; } // Optional, for driver
}

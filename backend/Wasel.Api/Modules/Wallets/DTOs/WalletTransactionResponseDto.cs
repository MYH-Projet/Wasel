using Wasel.Api.Modules.Wallets.Enums;

namespace Wasel.Api.Modules.Wallets.DTOs;

public class WalletTransactionResponseDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public WalletTransactionDirection Direction { get; set; }
    public WalletTransactionReason Reason { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid? DeliveryId { get; set; }
}

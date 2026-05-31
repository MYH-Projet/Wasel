using Wasel.Api.Modules.Deliveries.Entities;
using Wasel.Api.Modules.Wallets.Enums;
using Wasel.Api.Shared.Common;

namespace Wasel.Api.Modules.Wallets.Entities;

public class WalletTransaction : BaseEntity
{
    public Guid WalletId { get; set; }
    public Wallet Wallet { get; set; } = default!;

    public decimal Amount { get; set; }
    
    public string Currency { get; set; } = "MAD";
    
    public WalletTransactionDirection Direction { get; set; }
    
    public WalletTransactionReason Reason { get; set; }
    
    public string Description { get; set; } = string.Empty;
    
    public Guid? DeliveryId { get; set; }
    public Delivery? Delivery { get; set; }
    
    public Guid? WalletTransferId { get; set; }
    public WalletTransfer? WalletTransfer { get; set; }
}

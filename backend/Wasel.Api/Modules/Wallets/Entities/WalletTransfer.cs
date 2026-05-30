using Wasel.Api.Modules.Wallets.Enums;
using Wasel.Api.Shared.Common;

namespace Wasel.Api.Modules.Wallets.Entities;

public class WalletTransfer : BaseEntity
{
    public Guid? FromWalletId { get; set; }
    public Wallet? FromWallet { get; set; }
    
    public Guid? ToWalletId { get; set; }
    public Wallet? ToWallet { get; set; }
    
    public decimal Amount { get; set; }
    
    public string Currency { get; set; } = "MAD";
    
    public WalletTransferType Type { get; set; }
    
    public WalletTransferStatus Status { get; set; } = WalletTransferStatus.PENDING;
    
    public ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
}

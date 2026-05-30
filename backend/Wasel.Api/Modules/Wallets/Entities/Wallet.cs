using Wasel.Api.Modules.Wallets.Enums;
using Wasel.Api.Shared.Common;

namespace Wasel.Api.Modules.Wallets.Entities;

public abstract class Wallet : BaseEntity
{
    public decimal Balance { get; set; } = 0;
    
    public string Currency { get; set; } = "MAD";
    
    public WalletStatus Status { get; set; } = WalletStatus.ACTIVE;
    
    public ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
}

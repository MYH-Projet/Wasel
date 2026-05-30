using Wasel.Api.Modules.Users.Entities;

namespace Wasel.Api.Modules.Wallets.Entities;

public class ClientWallet : Wallet
{
    public Guid UserId { get; set; }
    
    public User User { get; set; } = default!;
}

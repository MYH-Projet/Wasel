using Wasel.Api.Modules.Drivers.Entities;

namespace Wasel.Api.Modules.Wallets.Entities;

public class DriverWallet : Wallet
{
    public Guid DriverId { get; set; }
    
    public Driver Driver { get; set; } = default!;
}

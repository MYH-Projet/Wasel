using Wasel.Api.Modules.Wallets.Entities;

namespace Wasel.Api.Modules.Wallets.Repositories;

public interface IWalletRepository
{
    Task<Wallet?> GetWalletByIdAsync(Guid id);
    Task<ClientWallet?> GetClientWalletByUserIdAsync(Guid userId);
    Task<DriverWallet?> GetDriverWalletByDriverIdAsync(Guid driverId);
    Task<PlatformWallet?> GetPlatformWalletAsync();
    
    Task<ClientWallet> CreateClientWalletAsync(ClientWallet wallet);
    Task<DriverWallet> CreateDriverWalletAsync(DriverWallet wallet);
    
    Task UpdateWalletAsync(Wallet wallet);
    
    Task AddTransactionAsync(WalletTransaction transaction);
    Task AddTransferAsync(WalletTransfer transfer);

    Task<(IEnumerable<WalletTransaction> Items, int TotalCount)> GetTransactionsByWalletIdAsync(Guid walletId, int page, int pageSize);
    Task<decimal> GetDriverMonthlyEarningsAsync(Guid driverId, int year, int month);
}

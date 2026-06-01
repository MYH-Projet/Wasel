using Wasel.Api.Modules.Wallets.DTOs;
using Wasel.Api.Modules.Wallets.Entities;
using Wasel.Api.Modules.Wallets.Enums;

namespace Wasel.Api.Modules.Wallets.Services;

public interface IWalletService
{
    Task<ClientWallet> EnsureClientWalletExistsAsync(Guid userId);
    Task<DriverWallet> EnsureDriverWalletExistsAsync(Guid driverId);
    
    Task<WalletBalanceResponseDto> GetClientWalletBalanceAsync(Guid userId);
    Task<WalletBalanceResponseDto> GetDriverWalletBalanceAsync(Guid driverId);
    
    Task<(IEnumerable<WalletTransactionResponseDto> Items, int TotalCount)> GetClientTransactionsAsync(Guid userId, int page, int pageSize);
    Task<(IEnumerable<WalletTransactionResponseDto> Items, int TotalCount)> GetDriverTransactionsAsync(Guid driverId, int page, int pageSize);
    
    Task ProcessDeliveryPaymentAsync(Guid clientId, Guid driverId, decimal amount, string currency, Guid deliveryId);
    
    Task WithdrawDriverFundsAsync(Guid driverId, WithdrawRequestDto request);
}

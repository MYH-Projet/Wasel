using Wasel.Api.Modules.Wallets.DTOs;
using Wasel.Api.Modules.Wallets.Entities;
using Wasel.Api.Modules.Wallets.Enums;
using Wasel.Api.Modules.Wallets.Repositories;
using Wasel.Api.Shared.Exceptions;
using Wasel.Api.Shared.Database;

namespace Wasel.Api.Modules.Wallets.Services;

public class WalletService : IWalletService
{
    private readonly IWalletRepository _walletRepository;
    private readonly WaselDbContext _context; // Required for transactions if orchestrating multiple repositories, but here it's fine since we save via Repo. Wait, we want atomic operations.
    // Actually, PaymentService orchestrates ProcessDeliveryPaymentAsync within its own transaction. We don't need a nested transaction here if called from PaymentService.
    // However, WithdrawDriverFundsAsync needs its own transaction since it's called directly from WalletsController.

    public WalletService(IWalletRepository walletRepository, WaselDbContext context)
    {
        _walletRepository = walletRepository;
        _context = context;
    }

    public async Task<ClientWallet> EnsureClientWalletExistsAsync(Guid userId)
    {
        var wallet = await _walletRepository.GetClientWalletByUserIdAsync(userId);
        if (wallet == null)
        {
            wallet = new ClientWallet
            {
                UserId = userId,
                Balance = 0,
                Currency = "MAD"
            };
            await _walletRepository.CreateClientWalletAsync(wallet);
        }
        return wallet;
    }

    public async Task<DriverWallet> EnsureDriverWalletExistsAsync(Guid driverId)
    {
        var wallet = await _walletRepository.GetDriverWalletByDriverIdAsync(driverId);
        if (wallet == null)
        {
            wallet = new DriverWallet
            {
                DriverId = driverId,
                Balance = 0,
                Currency = "MAD"
            };
            await _walletRepository.CreateDriverWalletAsync(wallet);
        }
        return wallet;
    }

    public async Task<WalletBalanceResponseDto> GetClientWalletBalanceAsync(Guid userId)
    {
        var wallet = await EnsureClientWalletExistsAsync(userId);
        return new WalletBalanceResponseDto
        {
            WalletId = wallet.Id,
            Balance = wallet.Balance,
            Currency = wallet.Currency,
            Status = wallet.Status
        };
    }

    public async Task<WalletBalanceResponseDto> GetDriverWalletBalanceAsync(Guid driverId)
    {
        var wallet = await EnsureDriverWalletExistsAsync(driverId);
        
        var now = DateTime.UtcNow;
        var earnings = await _walletRepository.GetDriverMonthlyEarningsAsync(driverId, now.Year, now.Month);
        
        return new WalletBalanceResponseDto
        {
            WalletId = wallet.Id,
            Balance = wallet.Balance,
            Currency = wallet.Currency,
            Status = wallet.Status,
            MonthlyEarnings = earnings
        };
    }

    public async Task<(IEnumerable<WalletTransactionResponseDto> Items, int TotalCount)> GetClientTransactionsAsync(Guid userId, int page, int pageSize)
    {
        var wallet = await EnsureClientWalletExistsAsync(userId);
        var result = await _walletRepository.GetTransactionsByWalletIdAsync(wallet.Id, page, pageSize);
        
        var dtos = result.Items.Select(MapToDto);
        return (dtos, result.TotalCount);
    }

    public async Task<(IEnumerable<WalletTransactionResponseDto> Items, int TotalCount)> GetDriverTransactionsAsync(Guid driverId, int page, int pageSize)
    {
        var wallet = await EnsureDriverWalletExistsAsync(driverId);
        var result = await _walletRepository.GetTransactionsByWalletIdAsync(wallet.Id, page, pageSize);
        
        var dtos = result.Items.Select(MapToDto);
        return (dtos, result.TotalCount);
    }

    public async Task ProcessDeliveryPaymentAsync(Guid clientId, Guid driverId, decimal amount, string currency, Guid deliveryId)
    {
        // This method assumes it's being called within an EF Core Transaction (initiated by PaymentService).
        
        var clientWallet = await EnsureClientWalletExistsAsync(clientId);
        var driverWallet = await EnsureDriverWalletExistsAsync(driverId);

        if (clientWallet.Balance < amount)
        {
            throw ApiException.BadRequest("Solde insuffisant dans le wallet client.");
        }

        clientWallet.Balance -= amount;
        driverWallet.Balance += amount;

        await _walletRepository.UpdateWalletAsync(clientWallet);
        await _walletRepository.UpdateWalletAsync(driverWallet);

        var transfer = new WalletTransfer
        {
            FromWalletId = clientWallet.Id,
            ToWalletId = driverWallet.Id,
            Amount = amount,
            Currency = currency,
            Type = WalletTransferType.DELIVERY_PAYMENT_SPLIT,
            Status = WalletTransferStatus.COMPLETED
        };
        await _walletRepository.AddTransferAsync(transfer);

        var clientTransaction = new WalletTransaction
        {
            WalletId = clientWallet.Id,
            Amount = amount,
            Currency = currency,
            Direction = WalletTransactionDirection.DEBIT,
            Reason = WalletTransactionReason.DELIVERY_PAYMENT,
            Description = "Paiement de livraison",
            DeliveryId = deliveryId,
            WalletTransferId = transfer.Id
        };

        var driverTransaction = new WalletTransaction
        {
            WalletId = driverWallet.Id,
            Amount = amount,
            Currency = currency,
            Direction = WalletTransactionDirection.CREDIT,
            Reason = WalletTransactionReason.DELIVERY_EARNING,
            Description = "Gain de livraison",
            DeliveryId = deliveryId,
            WalletTransferId = transfer.Id
        };

        await _walletRepository.AddTransactionAsync(clientTransaction);
        await _walletRepository.AddTransactionAsync(driverTransaction);
    }

    public async Task WithdrawDriverFundsAsync(Guid driverId, WithdrawRequestDto request)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var wallet = await EnsureDriverWalletExistsAsync(driverId);

            if (wallet.Balance < request.Amount)
            {
                throw ApiException.BadRequest("Solde insuffisant pour ce retrait.");
            }

            // TODO: Move MIN_WITHDRAWAL_AMOUNT to configuration / appsettings
            const decimal MIN_WITHDRAWAL_AMOUNT = 50.0m;
            if (request.Amount < MIN_WITHDRAWAL_AMOUNT)
            {
                throw ApiException.BadRequest($"Le montant de retrait minimum est de {MIN_WITHDRAWAL_AMOUNT} {request.Currency}.");
            }

            wallet.Balance -= request.Amount;
            await _walletRepository.UpdateWalletAsync(wallet);

            var transfer = new WalletTransfer
            {
                FromWalletId = wallet.Id,
                ToWalletId = null, // External bank transfer
                Amount = request.Amount,
                Currency = request.Currency,
                Type = WalletTransferType.WITHDRAWAL,
                Status = WalletTransferStatus.PENDING // Manual external action required
            };
            await _walletRepository.AddTransferAsync(transfer);

            var transactionEntity = new WalletTransaction
            {
                WalletId = wallet.Id,
                Amount = request.Amount,
                Currency = request.Currency,
                Direction = WalletTransactionDirection.DEBIT,
                Reason = WalletTransactionReason.DRIVER_WITHDRAWAL,
                Description = "Demande de retrait de fonds",
                WalletTransferId = transfer.Id
            };
            await _walletRepository.AddTransactionAsync(transactionEntity);

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private WalletTransactionResponseDto MapToDto(WalletTransaction t)
    {
        return new WalletTransactionResponseDto
        {
            Id = t.Id,
            Amount = t.Amount,
            Currency = t.Currency,
            Direction = t.Direction,
            Reason = t.Reason,
            Description = t.Description,
            CreatedAt = t.CreatedAt,
            DeliveryId = t.DeliveryId
        };
    }
}
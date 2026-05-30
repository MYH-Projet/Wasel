using Microsoft.EntityFrameworkCore;
using Wasel.Api.Modules.Wallets.Entities;
using Wasel.Api.Modules.Wallets.Enums;
using Wasel.Api.Shared.Database;

namespace Wasel.Api.Modules.Wallets.Repositories;

public class WalletRepository : IWalletRepository
{
    private readonly WaselDbContext _context;

    public WalletRepository(WaselDbContext context)
    {
        _context = context;
    }

    public async Task<Wallet?> GetWalletByIdAsync(Guid id)
    {
        return await _context.Wallets.FindAsync(id);
    }

    public async Task<ClientWallet?> GetClientWalletByUserIdAsync(Guid userId)
    {
        return await _context.ClientWallets.FirstOrDefaultAsync(w => w.UserId == userId);
    }

    public async Task<DriverWallet?> GetDriverWalletByDriverIdAsync(Guid driverId)
    {
        return await _context.DriverWallets.FirstOrDefaultAsync(w => w.DriverId == driverId);
    }

    public async Task<PlatformWallet?> GetPlatformWalletAsync()
    {
        return await _context.PlatformWallets.FirstOrDefaultAsync();
    }

    public async Task<ClientWallet> CreateClientWalletAsync(ClientWallet wallet)
    {
        _context.ClientWallets.Add(wallet);
        await _context.SaveChangesAsync();
        return wallet;
    }

    public async Task<DriverWallet> CreateDriverWalletAsync(DriverWallet wallet)
    {
        _context.DriverWallets.Add(wallet);
        await _context.SaveChangesAsync();
        return wallet;
    }

    public async Task UpdateWalletAsync(Wallet wallet)
    {
        _context.Wallets.Update(wallet);
        await _context.SaveChangesAsync();
    }

    public async Task AddTransactionAsync(WalletTransaction transaction)
    {
        _context.WalletTransactions.Add(transaction);
        await _context.SaveChangesAsync();
    }

    public async Task AddTransferAsync(WalletTransfer transfer)
    {
        _context.WalletTransfers.Add(transfer);
        await _context.SaveChangesAsync();
    }

    public async Task<(IEnumerable<WalletTransaction> Items, int TotalCount)> GetTransactionsByWalletIdAsync(Guid walletId, int page, int pageSize)
    {
        var query = _context.WalletTransactions
            .Where(t => t.WalletId == walletId)
            .OrderByDescending(t => t.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return (items, totalCount);
    }

    public async Task<decimal> GetDriverMonthlyEarningsAsync(Guid driverId, int year, int month)
    {
        var wallet = await GetDriverWalletByDriverIdAsync(driverId);
        if (wallet == null) return 0;

        return await _context.WalletTransactions
            .Where(t => t.WalletId == wallet.Id 
                     && t.Reason == WalletTransactionReason.DELIVERY_EARNING
                     && t.Direction == WalletTransactionDirection.CREDIT
                     && t.CreatedAt.Year == year 
                     && t.CreatedAt.Month == month)
            .SumAsync(t => t.Amount);
    }
}

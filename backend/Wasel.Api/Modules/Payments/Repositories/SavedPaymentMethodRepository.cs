using Microsoft.EntityFrameworkCore;
using Wasel.Api.Modules.Payments.Entities;
using Wasel.Api.Shared.Database;

namespace Wasel.Api.Modules.Payments.Repositories;

public class SavedPaymentMethodRepository : ISavedPaymentMethodRepository
{
    private readonly WaselDbContext _context;

    public SavedPaymentMethodRepository(WaselDbContext context)
    {
        _context = context;
    }

    public async Task<SavedPaymentMethod?> GetByIdAsync(Guid id)
    {
        return await _context.SavedPaymentMethods.FindAsync(id);
    }

    public async Task<IEnumerable<SavedPaymentMethod>> GetByUserIdAsync(Guid userId)
    {
        return await _context.SavedPaymentMethods
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<SavedPaymentMethod> CreateAsync(SavedPaymentMethod paymentMethod)
    {
        _context.SavedPaymentMethods.Add(paymentMethod);
        await _context.SaveChangesAsync();
        return paymentMethod;
    }

    public async Task UpdateAsync(SavedPaymentMethod paymentMethod)
    {
        _context.SavedPaymentMethods.Update(paymentMethod);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(SavedPaymentMethod paymentMethod)
    {
        _context.SavedPaymentMethods.Remove(paymentMethod);
        await _context.SaveChangesAsync();
    }

    public async Task SetAllToNotDefaultAsync(Guid userId)
    {
        var methods = await _context.SavedPaymentMethods.Where(s => s.UserId == userId).ToListAsync();
        foreach (var method in methods)
        {
            method.IsDefault = false;
        }
        await _context.SaveChangesAsync();
    }
}

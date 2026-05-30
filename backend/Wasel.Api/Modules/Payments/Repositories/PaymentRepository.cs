using Microsoft.EntityFrameworkCore;
using Wasel.Api.Modules.Payments.Entities;
using Wasel.Api.Shared.Database;

namespace Wasel.Api.Modules.Payments.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly WaselDbContext _context;

    public PaymentRepository(WaselDbContext context)
    {
        _context = context;
    }

    public async Task<Payment?> GetPaymentByIdAsync(Guid id)
    {
        return await _context.Payments.FindAsync(id);
    }

    public async Task<Payment?> GetPaymentByDeliveryIdAsync(Guid deliveryId)
    {
        return await _context.Payments.FirstOrDefaultAsync(p => p.DeliveryId == deliveryId);
    }

    public async Task<Payment> CreatePaymentAsync(Payment payment)
    {
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task UpdatePaymentAsync(Payment payment)
    {
        _context.Payments.Update(payment);
        await _context.SaveChangesAsync();
    }
}

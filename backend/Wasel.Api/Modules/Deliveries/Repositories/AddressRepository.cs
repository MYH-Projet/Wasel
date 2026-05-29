using Microsoft.EntityFrameworkCore;
using Wasel.Api.Modules.Deliveries.Entities;
using Wasel.Api.Shared.Database;

namespace Wasel.Api.Modules.Deliveries.Repositories;

public class AddressRepository : IAddressRepository
{
    private readonly WaselDbContext _context;

    public AddressRepository(WaselDbContext context)
    {
        _context = context;
    }

    public async Task<Address> AddAsync(Address address)
    {
        _context.Addresses.Add(address);
        await _context.SaveChangesAsync();
        return address;
    }

    public async Task<List<Address>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Addresses
            .Where(a => a.ClientId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<Address?> GetByIdAsync(Guid id)
    {
        return await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<bool> IsLinkedToDeliveryAsync(Guid addressId)
    {
        return await _context.Deliveries.AnyAsync(d =>
            d.PickupAddressId == addressId ||
            d.DropoffAddressId == addressId
        );
    }

    public async Task DeleteAsync(Address address)
    {
        _context.Addresses.Remove(address);
        await _context.SaveChangesAsync();
    }
}
using Wasel.Api.Modules.Deliveries.Entities;

namespace Wasel.Api.Modules.Deliveries.Repositories;

public interface IAddressRepository
{
    Task<Address> AddAsync(Address address);
    Task<List<Address>> GetByUserIdAsync(Guid userId);
    Task<Address?> GetByIdAsync(Guid id);
    Task<bool> IsLinkedToDeliveryAsync(Guid addressId);
    Task DeleteAsync(Address address);
}
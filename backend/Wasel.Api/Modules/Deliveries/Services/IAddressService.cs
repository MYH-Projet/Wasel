using Wasel.Api.Modules.Deliveries.DTOs;

namespace Wasel.Api.Modules.Deliveries.Services;

public interface IAddressService
{
    Task<AddressResponseDto> CreateAsync(Guid clientId, CreateAddressRequestDto request);

    Task<List<AddressResponseDto>> GetMyAddressesAsync(Guid clientId);

    Task DeleteAsync(Guid clientId, Guid addressId);
}
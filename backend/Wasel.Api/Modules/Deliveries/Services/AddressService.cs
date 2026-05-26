using Wasel.Api.Modules.Deliveries.DTOs;
using Wasel.Api.Modules.Deliveries.Entities;
using Wasel.Api.Modules.Deliveries.Repositories;

namespace Wasel.Api.Modules.Deliveries.Services;

public class AddressService : IAddressService
{
    private readonly IAddressRepository _addressRepository;

    public AddressService(IAddressRepository addressRepository)
    {
        _addressRepository = addressRepository;
    }

    public async Task<AddressResponseDto> CreateAsync(Guid clientId, CreateAddressRequestDto request)
    {
        var address = new Address
        {
            ClientId = clientId,
            Label = request.Label,
            Street = request.Street,
            City = request.City,
            PostalCode = request.PostalCode,
            Country = request.Country,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            AdditionalInfo = request.AdditionalInfo,
            Instructions = request.Instructions
        };

        var createdAddress = await _addressRepository.AddAsync(address);

        return MapToDto(createdAddress);
    }

    public async Task<List<AddressResponseDto>> GetMyAddressesAsync(Guid clientId)
    {
        var addresses = await _addressRepository.GetByUserIdAsync(clientId);

        return addresses.Select(MapToDto).ToList();
    }

    public async Task DeleteAsync(Guid clientId, Guid addressId)
    {
        var address = await _addressRepository.GetByIdAsync(addressId);

        if (address is null)
            throw new KeyNotFoundException("Adresse introuvable.");

        if (address.ClientId != clientId)
            throw new UnauthorizedAccessException("Accès refusé à cette adresse.");

        var isLinkedToDelivery = await _addressRepository.IsLinkedToDeliveryAsync(addressId);

        if (isLinkedToDelivery)
            throw new InvalidOperationException("Cette adresse est liée à une livraison existante.");

        await _addressRepository.DeleteAsync(address);
    }

    private static AddressResponseDto MapToDto(Address address)
    {
        return new AddressResponseDto
        {
            Id = address.Id,
            Label = address.Label,
            Street = address.Street,
            City = address.City,
            PostalCode = address.PostalCode,
            Country = address.Country,
            Latitude = address.Latitude,
            Longitude = address.Longitude,
            AdditionalInfo = address.AdditionalInfo,
            Instructions = address.Instructions,
            CreatedAt = address.CreatedAt
        };
    }
}
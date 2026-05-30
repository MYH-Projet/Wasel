using Wasel.Api.Modules.Payments.DTOs;
using Wasel.Api.Modules.Payments.Entities;
using Wasel.Api.Modules.Payments.Repositories;
using Wasel.Api.Shared.Exceptions;

namespace Wasel.Api.Modules.Payments.Services;

public class PaymentMethodService : IPaymentMethodService
{
    private readonly ISavedPaymentMethodRepository _repository;

    public PaymentMethodService(ISavedPaymentMethodRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaymentMethodResponseDto> CreateAsync(CreatePaymentMethodRequestDto request, Guid userId)
    {
        if (request.IsDefault)
        {
            await _repository.SetAllToNotDefaultAsync(userId);
        }

        var entity = new SavedPaymentMethod
        {
            UserId = userId,
            ProviderName = request.ProviderName,
            ProviderCustomerId = request.ProviderCustomerId,
            ProviderPaymentMethodId = request.ProviderPaymentMethodId,
            CardBrand = request.CardBrand,
            CardLast4 = request.CardLast4,
            IsDefault = request.IsDefault
        };

        var created = await _repository.CreateAsync(entity);
        return MapToDto(created);
    }

    public async Task<IEnumerable<PaymentMethodResponseDto>> GetMyPaymentMethodsAsync(Guid userId)
    {
        var items = await _repository.GetByUserIdAsync(userId);
        return items.Select(MapToDto);
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw ApiException.NotFound("Moyen de paiement introuvable.");
        if (entity.UserId != userId) throw ApiException.Forbidden("Accès refusé.");

        await _repository.DeleteAsync(entity);
    }

    public async Task SetDefaultAsync(Guid id, Guid userId)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw ApiException.NotFound("Moyen de paiement introuvable.");
        if (entity.UserId != userId) throw ApiException.Forbidden("Accès refusé.");

        await _repository.SetAllToNotDefaultAsync(userId);
        
        entity.IsDefault = true;
        await _repository.UpdateAsync(entity);
    }

    private PaymentMethodResponseDto MapToDto(SavedPaymentMethod s)
    {
        return new PaymentMethodResponseDto
        {
            Id = s.Id,
            ProviderName = s.ProviderName,
            CardBrand = s.CardBrand,
            CardLast4 = s.CardLast4,
            IsDefault = s.IsDefault,
            CreatedAt = s.CreatedAt
        };
    }
}

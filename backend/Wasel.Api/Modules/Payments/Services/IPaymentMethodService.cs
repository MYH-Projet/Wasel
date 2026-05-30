using Wasel.Api.Modules.Payments.DTOs;

namespace Wasel.Api.Modules.Payments.Services;

public interface IPaymentMethodService
{
    Task<PaymentMethodResponseDto> CreateAsync(CreatePaymentMethodRequestDto request, Guid userId);
    Task<IEnumerable<PaymentMethodResponseDto>> GetMyPaymentMethodsAsync(Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
    Task SetDefaultAsync(Guid id, Guid userId);
}

using Wasel.Api.Modules.Payments.DTOs;

namespace Wasel.Api.Modules.Payments.Services;

public interface IPaymentService
{
    Task<InitiatePaymentResponseDto> InitiatePaymentAsync(InitiatePaymentRequestDto request, Guid currentUserId);
    Task<ConfirmCashPaymentResponseDto> ConfirmCashPaymentAsync(Guid paymentId, Guid currentUserId);
    Task<PaymentDetailsResponseDto> GetPaymentDetailsAsync(Guid deliveryId, Guid currentUserId, IEnumerable<string> currentUserRoles);
}

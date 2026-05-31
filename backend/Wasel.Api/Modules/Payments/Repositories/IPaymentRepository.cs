using Wasel.Api.Modules.Payments.Entities;

namespace Wasel.Api.Modules.Payments.Repositories;

public interface IPaymentRepository
{
    Task<Payment?> GetPaymentByIdAsync(Guid id);
    Task<Payment?> GetPaymentByDeliveryIdAsync(Guid deliveryId);
    Task<Payment> CreatePaymentAsync(Payment payment);
    Task UpdatePaymentAsync(Payment payment);
}

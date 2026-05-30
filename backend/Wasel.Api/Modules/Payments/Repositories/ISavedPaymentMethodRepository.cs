using Wasel.Api.Modules.Payments.Entities;

namespace Wasel.Api.Modules.Payments.Repositories;

public interface ISavedPaymentMethodRepository
{
    Task<SavedPaymentMethod?> GetByIdAsync(Guid id);
    Task<IEnumerable<SavedPaymentMethod>> GetByUserIdAsync(Guid userId);
    Task<SavedPaymentMethod> CreateAsync(SavedPaymentMethod paymentMethod);
    Task UpdateAsync(SavedPaymentMethod paymentMethod);
    Task DeleteAsync(SavedPaymentMethod paymentMethod);
    Task SetAllToNotDefaultAsync(Guid userId);
}

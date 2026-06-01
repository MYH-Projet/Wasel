using Wasel.Api.Modules.Users.Entities;
using Wasel.Api.Shared.Common;

namespace Wasel.Api.Modules.Payments.Entities;

public class SavedPaymentMethod : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public string ProviderName { get; set; } = string.Empty;
    
    public string ProviderCustomerId { get; set; } = string.Empty;
    
    public string ProviderPaymentMethodId { get; set; } = string.Empty;
    
    public string CardBrand { get; set; } = string.Empty;
    
    public string CardLast4 { get; set; } = string.Empty;
    
    public bool IsDefault { get; set; } = false;
}

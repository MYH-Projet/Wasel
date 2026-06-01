using Wasel.Api.Modules.Deliveries.Entities;
using Wasel.Api.Modules.Deliveries.Enums;
using Wasel.Api.Modules.Payments.Enums;
using Wasel.Api.Shared.Common;

namespace Wasel.Api.Modules.Payments.Entities;

public class Payment : BaseEntity
{
    public Guid DeliveryId { get; set; }
    public Delivery Delivery { get; set; } = default!;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "MAD";

    public PaymentMethod Method { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.PENDING;

    public string TransactionReference { get; set; } = string.Empty;
}

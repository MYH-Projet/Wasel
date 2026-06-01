namespace Wasel.Api.Modules.Payments.Services;

public interface IPaymentGateway
{
    Task<string> GeneratePaymentUrlAsync(Guid paymentId, decimal amount, string currency);
}

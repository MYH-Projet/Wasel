namespace Wasel.Api.Modules.Payments.Services;

public class FakePaymentGateway : IPaymentGateway
{
    public Task<string> GeneratePaymentUrlAsync(Guid paymentId, decimal amount, string currency)
    {
        // Simulate generating a hosted payment page URL
        var url = $"https://sandbox.payment.local/pay/{paymentId}?amount={amount}&currency={currency}";
        return Task.FromResult(url);
    }
}

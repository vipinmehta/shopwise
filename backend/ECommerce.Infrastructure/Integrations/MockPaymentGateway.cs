namespace ECommerce.Infrastructure.Integrations;

public class MockPaymentGateway : IPaymentGateway
{
    public Task<PaymentResult> ChargeAsync(decimal amount, string currency, string orderNumber)
    {
        var reference = $"MOCK-{orderNumber}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
        return Task.FromResult(new PaymentResult(true, reference, null));
    }
}

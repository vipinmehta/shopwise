namespace ECommerce.Infrastructure.Integrations;

public record PaymentResult(bool Success, string TransactionReference, string? FailureReason);

/// <summary>Swap the registered implementation with a real Razorpay/Stripe client to go live.</summary>
public interface IPaymentGateway
{
    Task<PaymentResult> ChargeAsync(decimal amount, string currency, string orderNumber);
}

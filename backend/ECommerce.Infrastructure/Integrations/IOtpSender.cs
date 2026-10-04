namespace ECommerce.Infrastructure.Integrations;

/// <summary>Swap the registered implementation with a real Twilio/MSG91 client to go live.</summary>
public interface IOtpSender
{
    Task SendAsync(string phone, string code);
}

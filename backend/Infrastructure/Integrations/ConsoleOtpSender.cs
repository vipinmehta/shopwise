using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Integrations;

public class ConsoleOtpSender : IOtpSender
{
    private readonly ILogger<ConsoleOtpSender> _logger;

    public ConsoleOtpSender(ILogger<ConsoleOtpSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string phone, string code)
    {
        _logger.LogInformation("[DEV OTP] {Phone} -> {Code} (would be sent via SMS in production)", phone, code);
        return Task.CompletedTask;
    }
}

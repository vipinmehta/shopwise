namespace ECommerce.Infrastructure.Integrations;

public record GoogleProfile(string GoogleId, string Email, string Name);

/// <summary>
/// Dev-mode implementation trusts a JSON payload instead of verifying a real Google JWT.
/// Swap for a real implementation (validate the JWT against Google's public keys with the
/// configured GoogleClientId, e.g. using Google.Apis.Auth) to go live.
/// </summary>
public interface IGoogleTokenValidator
{
    Task<GoogleProfile> ValidateAsync(string idToken);
}

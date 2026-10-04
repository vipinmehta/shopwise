using System.Text;
using System.Text.Json;
using ECommerce.Application.Common;

namespace ECommerce.Infrastructure.Integrations;

/// <summary>
/// Accepts a base64-encoded JSON payload {"sub","email","name"} in place of a real Google ID
/// token. The Angular Google Sign-In button is wired to send the real JWT here once a
/// GoogleClientId is configured and this class is replaced with real verification.
/// </summary>
public class DevGoogleTokenValidator : IGoogleTokenValidator
{
    public Task<GoogleProfile> ValidateAsync(string idToken)
    {
        try
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(idToken));
            var payload = JsonSerializer.Deserialize<JsonElement>(json);
            var sub = payload.GetProperty("sub").GetString() ?? throw new AppException("Invalid Google token payload");
            var email = payload.GetProperty("email").GetString() ?? throw new AppException("Invalid Google token payload");
            var name = payload.TryGetProperty("name", out var n) ? n.GetString() ?? email : email;
            return Task.FromResult(new GoogleProfile(sub, email, name));
        }
        catch (Exception ex) when (ex is not AppException)
        {
            throw new AppException("Invalid Google token", 401);
        }
    }
}

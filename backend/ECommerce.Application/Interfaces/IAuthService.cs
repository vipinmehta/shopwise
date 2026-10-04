using ECommerce.Application.Dtos;

namespace ECommerce.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task RequestOtpAsync(OtpRequestDto request);
    Task<AuthResponse> VerifyOtpAsync(OtpVerifyDto request);
    Task<AuthResponse> GoogleLoginAsync(GoogleLoginRequest request);
    Task<UserProfileDto> GetProfileAsync(string userId);
}

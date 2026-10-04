namespace ECommerce.Application.Dtos;

public record RegisterRequest(string Name, string Email, string Password, string? Phone);
public record LoginRequest(string Email, string Password);
public record OtpRequestDto(string Phone);
public record OtpVerifyDto(string Phone, string Code, string? Name);
public record GoogleLoginRequest(string IdToken);

public record AuthResponse(string Token, string UserId, string Name, string Email, string Role);

public record UserProfileDto(string Id, string Name, string Email, string? Phone, string Role);

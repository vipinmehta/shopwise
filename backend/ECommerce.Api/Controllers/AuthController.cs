using ECommerce.Api.Common;
using ECommerce.Application.Dtos;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request) =>
        Ok(await _authService.RegisterAsync(request));

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request) =>
        Ok(await _authService.LoginAsync(request));

    [HttpPost("otp/request")]
    public async Task<IActionResult> RequestOtp(OtpRequestDto request)
    {
        await _authService.RequestOtpAsync(request);
        return Ok(new { message = "OTP sent" });
    }

    [HttpPost("otp/verify")]
    public async Task<ActionResult<AuthResponse>> VerifyOtp(OtpVerifyDto request) =>
        Ok(await _authService.VerifyOtpAsync(request));

    [HttpPost("google")]
    public async Task<ActionResult<AuthResponse>> GoogleLogin(GoogleLoginRequest request) =>
        Ok(await _authService.GoogleLoginAsync(request));

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserProfileDto>> Me() =>
        Ok(await _authService.GetProfileAsync(this.GetUserId()));
}

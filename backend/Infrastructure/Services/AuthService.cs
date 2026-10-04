using ECommerce.Application.Common;
using ECommerce.Application.Dtos;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Integrations;
using ECommerce.Infrastructure.Persistence;
using MongoDB.Driver;

namespace ECommerce.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly MongoDbContext _db;
    private readonly ITokenService _tokenService;
    private readonly IOtpSender _otpSender;
    private readonly IGoogleTokenValidator _googleValidator;

    public AuthService(MongoDbContext db, ITokenService tokenService, IOtpSender otpSender, IGoogleTokenValidator googleValidator)
    {
        _db = db;
        _tokenService = tokenService;
        _otpSender = otpSender;
        _googleValidator = googleValidator;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var existing = await _db.Users.Find(u => u.Email == request.Email).FirstOrDefaultAsync();
        if (existing is not null)
        {
            throw new AppException("An account with this email already exists");
        }

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Phone = request.Phone,
            Role = UserRole.Customer
        };
        await _db.Users.InsertOneAsync(user);

        return BuildAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _db.Users.Find(u => u.Email == request.Email).FirstOrDefaultAsync();
        if (user is null || user.PasswordHash is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new AppException("Invalid email or password", 401);
        }
        if (!user.IsActive)
        {
            throw new AppException("This account has been disabled", 403);
        }

        return BuildAuthResponse(user);
    }

    public async Task RequestOtpAsync(OtpRequestDto request)
    {
        var code = Random.Shared.Next(0, 1000000).ToString("D6");
        var challenge = new OtpChallenge
        {
            Phone = request.Phone,
            Code = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };
        await _db.OtpChallenges.InsertOneAsync(challenge);
        await _otpSender.SendAsync(request.Phone, code);
    }

    public async Task<AuthResponse> VerifyOtpAsync(OtpVerifyDto request)
    {
        var challenge = await _db.OtpChallenges
            .Find(o => o.Phone == request.Phone && !o.Consumed)
            .SortByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();

        if (challenge is null || challenge.ExpiresAt < DateTime.UtcNow)
        {
            throw new AppException("OTP has expired, please request a new one", 401);
        }
        if (challenge.Attempts >= 5)
        {
            throw new AppException("Too many attempts, please request a new OTP", 429);
        }
        if (challenge.Code != request.Code)
        {
            await _db.OtpChallenges.UpdateOneAsync(o => o.Id == challenge.Id, Builders<OtpChallenge>.Update.Inc(o => o.Attempts, 1));
            throw new AppException("Incorrect OTP", 401);
        }

        await _db.OtpChallenges.UpdateOneAsync(o => o.Id == challenge.Id, Builders<OtpChallenge>.Update.Set(o => o.Consumed, true));

        var user = await _db.Users.Find(u => u.Phone == request.Phone).FirstOrDefaultAsync();
        if (user is null)
        {
            user = new User
            {
                Name = request.Name ?? request.Phone,
                Email = $"{request.Phone}@phone.local",
                Phone = request.Phone,
                PhoneVerified = true,
                Role = UserRole.Customer
            };
            await _db.Users.InsertOneAsync(user);
        }
        else if (!user.PhoneVerified)
        {
            await _db.Users.UpdateOneAsync(u => u.Id == user.Id, Builders<User>.Update.Set(u => u.PhoneVerified, true));
        }

        return BuildAuthResponse(user);
    }

    public async Task<AuthResponse> GoogleLoginAsync(GoogleLoginRequest request)
    {
        var profile = await _googleValidator.ValidateAsync(request.IdToken);

        var user = await _db.Users.Find(u => u.GoogleId == profile.GoogleId || u.Email == profile.Email).FirstOrDefaultAsync();
        if (user is null)
        {
            user = new User
            {
                Name = profile.Name,
                Email = profile.Email,
                GoogleId = profile.GoogleId,
                EmailVerified = true,
                Role = UserRole.Customer
            };
            await _db.Users.InsertOneAsync(user);
        }
        else if (user.GoogleId is null)
        {
            await _db.Users.UpdateOneAsync(u => u.Id == user.Id, Builders<User>.Update.Set(u => u.GoogleId, profile.GoogleId));
        }

        return BuildAuthResponse(user);
    }

    public async Task<UserProfileDto> GetProfileAsync(string userId)
    {
        var user = await _db.Users.Find(u => u.Id == userId).FirstOrDefaultAsync()
            ?? throw new AppException("User not found", 404);
        return new UserProfileDto(user.Id, user.Name, user.Email, user.Phone, user.Role.ToString());
    }

    private AuthResponse BuildAuthResponse(User user)
    {
        var token = _tokenService.GenerateToken(user);
        return new AuthResponse(token, user.Id, user.Name, user.Email, user.Role.ToString());
    }
}

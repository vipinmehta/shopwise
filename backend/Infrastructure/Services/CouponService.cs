using ECommerce.Application.Common;
using ECommerce.Application.Dtos;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using MongoDB.Driver;

namespace ECommerce.Infrastructure.Services;

public class CouponService : ICouponService
{
    private readonly MongoDbContext _db;

    public CouponService(MongoDbContext db)
    {
        _db = db;
    }

    public async Task<List<CouponDto>> GetAllAsync()
    {
        var coupons = await _db.Coupons.Find(FilterDefinition<Coupon>.Empty).ToListAsync();
        return coupons.Select(ToDto).ToList();
    }

    public async Task<CouponDto> CreateAsync(UpsertCouponRequest request)
    {
        var coupon = new Coupon
        {
            Code = request.Code.ToUpperInvariant(),
            Type = Enum.Parse<CouponType>(request.Type, true),
            Value = request.Value,
            MinOrderValue = request.MinOrderValue,
            ExpiresAt = request.ExpiresAt,
            UsageLimit = request.UsageLimit,
            IsActive = request.IsActive
        };
        await _db.Coupons.InsertOneAsync(coupon);
        return ToDto(coupon);
    }

    public async Task<CouponDto> UpdateAsync(string id, UpsertCouponRequest request)
    {
        var update = Builders<Coupon>.Update
            .Set(c => c.Code, request.Code.ToUpperInvariant())
            .Set(c => c.Type, Enum.Parse<CouponType>(request.Type, true))
            .Set(c => c.Value, request.Value)
            .Set(c => c.MinOrderValue, request.MinOrderValue)
            .Set(c => c.ExpiresAt, request.ExpiresAt)
            .Set(c => c.UsageLimit, request.UsageLimit)
            .Set(c => c.IsActive, request.IsActive);

        var coupon = await _db.Coupons.FindOneAndUpdateAsync(
            c => c.Id == id, update,
            new FindOneAndUpdateOptions<Coupon> { ReturnDocument = ReturnDocument.After });

        if (coupon is null) throw new AppException("Coupon not found", 404);
        return ToDto(coupon);
    }

    public async Task DeleteAsync(string id)
    {
        await _db.Coupons.DeleteOneAsync(c => c.Id == id);
    }

    public async Task<ValidateCouponResponse> ValidateAsync(ValidateCouponRequest request)
    {
        var coupon = await _db.Coupons.Find(c => c.Code == request.Code.ToUpperInvariant() && c.IsActive).FirstOrDefaultAsync();
        if (coupon is null)
        {
            return new ValidateCouponResponse(false, 0, "Coupon not found");
        }
        if (coupon.ExpiresAt.HasValue && coupon.ExpiresAt < DateTime.UtcNow)
        {
            return new ValidateCouponResponse(false, 0, "Coupon has expired");
        }
        if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit)
        {
            return new ValidateCouponResponse(false, 0, "Coupon usage limit reached");
        }
        if (request.OrderValue < coupon.MinOrderValue)
        {
            return new ValidateCouponResponse(false, 0, $"Minimum order value is {coupon.MinOrderValue}");
        }

        var discount = coupon.Type == CouponType.Percentage
            ? Math.Round(request.OrderValue * coupon.Value / 100m, 2)
            : coupon.Value;
        discount = Math.Min(discount, request.OrderValue);

        return new ValidateCouponResponse(true, discount, null);
    }

    private static CouponDto ToDto(Coupon c) => new(c.Id, c.Code, c.Type.ToString(), c.Value, c.MinOrderValue, c.ExpiresAt, c.UsageLimit, c.UsedCount, c.IsActive);
}

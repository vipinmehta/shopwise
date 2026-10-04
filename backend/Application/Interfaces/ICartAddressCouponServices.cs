using ECommerce.Application.Dtos;

namespace ECommerce.Application.Interfaces;

public interface ICartService
{
    Task<CartDto> GetCartAsync(string userId);
    Task<CartDto> AddItemAsync(string userId, AddCartItemRequest request);
    Task<CartDto> UpdateItemAsync(string userId, UpdateCartItemRequest request);
    Task<CartDto> RemoveItemAsync(string userId, string productId, string? size);
    Task<CartDto> SaveForLaterAsync(string userId, string productId, string? size);
    Task<CartDto> MoveToCartAsync(string userId, string productId, string? size);
    Task<CartDto> ToggleWishlistAsync(string userId, string productId);
}

public interface IAddressService
{
    Task<List<AddressDto>> GetAllAsync(string userId);
    Task<AddressDto> CreateAsync(string userId, UpsertAddressRequest request);
    Task<AddressDto> UpdateAsync(string userId, string id, UpsertAddressRequest request);
    Task DeleteAsync(string userId, string id);
}

public interface ICouponService
{
    Task<List<CouponDto>> GetAllAsync();
    Task<CouponDto> CreateAsync(UpsertCouponRequest request);
    Task<CouponDto> UpdateAsync(string id, UpsertCouponRequest request);
    Task DeleteAsync(string id);
    Task<ValidateCouponResponse> ValidateAsync(ValidateCouponRequest request);
}

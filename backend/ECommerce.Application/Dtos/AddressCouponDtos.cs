namespace ECommerce.Application.Dtos;

public record AddressDto(string Id, string FullName, string Phone, string Line1, string? Line2, string City, string State, string Pincode, string Country, bool IsDefault);
public record UpsertAddressRequest(string FullName, string Phone, string Line1, string? Line2, string City, string State, string Pincode, string Country, bool IsDefault);

public record CouponDto(string Id, string Code, string Type, decimal Value, decimal MinOrderValue, DateTime? ExpiresAt, int? UsageLimit, int UsedCount, bool IsActive);
public record UpsertCouponRequest(string Code, string Type, decimal Value, decimal MinOrderValue, DateTime? ExpiresAt, int? UsageLimit, bool IsActive);
public record ValidateCouponRequest(string Code, decimal OrderValue);
public record ValidateCouponResponse(bool Valid, decimal Discount, string? Message);

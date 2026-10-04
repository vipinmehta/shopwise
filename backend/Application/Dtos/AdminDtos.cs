namespace ECommerce.Application.Dtos;

public record AdminUserDto(string Id, string Name, string Email, string? Phone, string Role, bool IsActive, DateTime CreatedAt);
public record StockAdjustmentRequest(string Size, int Delta);

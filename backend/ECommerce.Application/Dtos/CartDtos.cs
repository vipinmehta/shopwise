namespace ECommerce.Application.Dtos;

public record CartItemDto(string ProductId, string ProductName, string? Thumbnail, string? Size, int Quantity, decimal UnitPrice, int AvailableStock);
public record CartDto(List<CartItemDto> Items, List<CartItemDto> SavedForLater, List<string> WishlistProductIds, decimal Subtotal);

public record AddCartItemRequest(string ProductId, string? Size, int Quantity);
public record UpdateCartItemRequest(string ProductId, string? Size, int Quantity);

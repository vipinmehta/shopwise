namespace ECommerce.Application.Dtos;

public record CheckoutRequest(string AddressId, string? CouponCode);

public record OrderItemDto(string ProductId, string ProductName, string? Image, string? Size, int Quantity, decimal UnitPrice);

public record OrderAddressDto(string FullName, string Phone, string Line1, string? Line2, string City, string State, string Pincode, string Country);

public record OrderDto(
    string Id, string OrderNumber, List<OrderItemDto> Items, OrderAddressDto ShippingAddress,
    string? CouponCode, decimal Subtotal, decimal Discount, decimal Total,
    string PaymentStatus, string Status, DateTime CreatedAt,
    List<OrderTrackingEventDto> TrackingHistory);

public record OrderTrackingEventDto(string Status, DateTime Timestamp, string? Note);

public record UpdateOrderStatusRequest(string Status, string? Note);
public record CancelOrderRequest(string? Reason);
public record ReturnOrderRequest(string? Reason);

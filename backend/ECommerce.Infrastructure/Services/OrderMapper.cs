using ECommerce.Application.Dtos;
using ECommerce.Domain.Entities;

namespace ECommerce.Infrastructure.Services;

internal static class OrderMapper
{
    public static OrderDto ToDto(Order o) => new(
        o.Id, o.OrderNumber,
        o.Items.Select(i => new OrderItemDto(i.ProductId, i.ProductName, i.Image, i.Size, i.Quantity, i.UnitPrice)).ToList(),
        new OrderAddressDto(o.ShippingAddress.FullName, o.ShippingAddress.Phone, o.ShippingAddress.Line1, o.ShippingAddress.Line2,
            o.ShippingAddress.City, o.ShippingAddress.State, o.ShippingAddress.Pincode, o.ShippingAddress.Country),
        o.CouponCode, o.Subtotal, o.Discount, o.Total,
        o.PaymentStatus.ToString(), o.Status.ToString(), o.CreatedAt,
        o.TrackingHistory.Select(t => new OrderTrackingEventDto(t.Status.ToString(), t.Timestamp, t.Note)).ToList());
}

using ECommerce.Application.Common;
using ECommerce.Application.Dtos;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Integrations;
using ECommerce.Infrastructure.Persistence;
using MongoDB.Driver;

namespace ECommerce.Infrastructure.Services;

public class CheckoutService : ICheckoutService
{
    private readonly MongoDbContext _db;
    private readonly IPaymentGateway _paymentGateway;

    public CheckoutService(MongoDbContext db, IPaymentGateway paymentGateway)
    {
        _db = db;
        _paymentGateway = paymentGateway;
    }

    public async Task<OrderDto> PlaceOrderAsync(string userId, CheckoutRequest request)
    {
        var cart = await _db.Carts.Find(c => c.UserId == userId).FirstOrDefaultAsync();
        if (cart is null || cart.Items.Count == 0)
        {
            throw new AppException("Your cart is empty");
        }

        var address = await _db.Addresses.Find(a => a.Id == request.AddressId && a.UserId == userId).FirstOrDefaultAsync()
            ?? throw new AppException("Address not found", 404);

        var productIds = cart.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _db.Products.Find(p => productIds.Contains(p.Id)).ToListAsync();
        var productMap = products.ToDictionary(p => p.Id);

        var orderItems = new List<OrderItem>();
        foreach (var item in cart.Items)
        {
            if (!productMap.TryGetValue(item.ProductId, out var product))
            {
                throw new AppException("A product in your cart is no longer available");
            }

            var variant = product.Variants.FirstOrDefault(v => v.Size == item.Size);
            if (variant is not null && variant.Stock < item.Quantity)
            {
                throw new AppException($"Insufficient stock for {product.Name} ({item.Size})");
            }

            var unitPrice = product.DiscountPrice ?? product.Price;
            orderItems.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Image = product.Images.FirstOrDefault(),
                Size = item.Size,
                Quantity = item.Quantity,
                UnitPrice = unitPrice
            });
        }

        var subtotal = orderItems.Sum(i => i.UnitPrice * i.Quantity);
        decimal discount = 0;
        Coupon? coupon = null;

        if (!string.IsNullOrWhiteSpace(request.CouponCode))
        {
            coupon = await _db.Coupons.Find(c => c.Code == request.CouponCode.ToUpperInvariant() && c.IsActive).FirstOrDefaultAsync()
                ?? throw new AppException("Invalid coupon code");
            if (coupon.ExpiresAt.HasValue && coupon.ExpiresAt < DateTime.UtcNow)
            {
                throw new AppException("Coupon has expired");
            }
            if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit)
            {
                throw new AppException("Coupon usage limit reached");
            }
            if (subtotal < coupon.MinOrderValue)
            {
                throw new AppException($"Minimum order value for this coupon is {coupon.MinOrderValue}");
            }
            discount = coupon.Type == CouponType.Percentage
                ? Math.Round(subtotal * coupon.Value / 100m, 2)
                : coupon.Value;
            discount = Math.Min(discount, subtotal);
        }

        var total = subtotal - discount;
        var orderNumber = $"ORD{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(100, 999)}";

        var paymentResult = await _paymentGateway.ChargeAsync(total, "INR", orderNumber);
        if (!paymentResult.Success)
        {
            throw new AppException(paymentResult.FailureReason ?? "Payment failed", 402);
        }

        var order = new Order
        {
            OrderNumber = orderNumber,
            UserId = userId,
            Items = orderItems,
            ShippingAddress = new OrderAddress
            {
                FullName = address.FullName,
                Phone = address.Phone,
                Line1 = address.Line1,
                Line2 = address.Line2,
                City = address.City,
                State = address.State,
                Pincode = address.Pincode,
                Country = address.Country
            },
            CouponCode = coupon?.Code,
            Subtotal = subtotal,
            Discount = discount,
            Total = total,
            PaymentStatus = PaymentStatus.Paid,
            PaymentReference = paymentResult.TransactionReference,
            Status = OrderStatus.Placed,
            TrackingHistory = new List<TrackingEvent> { new() { Status = OrderStatus.Placed, Note = "Order placed" } }
        };
        await _db.Orders.InsertOneAsync(order);

        foreach (var item in cart.Items)
        {
            var product = productMap[item.ProductId];
            var variant = product.Variants.FirstOrDefault(v => v.Size == item.Size);
            if (variant is not null)
            {
                variant.Stock = Math.Max(0, variant.Stock - item.Quantity);
                await _db.Products.ReplaceOneAsync(p => p.Id == product.Id, product);
            }
        }

        if (coupon is not null)
        {
            await _db.Coupons.UpdateOneAsync(c => c.Id == coupon.Id, Builders<Coupon>.Update.Inc(c => c.UsedCount, 1));
        }

        cart.Items.Clear();
        await _db.Carts.ReplaceOneAsync(c => c.Id == cart.Id, cart);

        return OrderMapper.ToDto(order);
    }
}

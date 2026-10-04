using ECommerce.Application.Common;
using ECommerce.Application.Dtos;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using MongoDB.Driver;

namespace ECommerce.Infrastructure.Services;

public class OrderService : IOrderService
{
    private static readonly HashSet<OrderStatus> CancellableStatuses = new() { OrderStatus.Placed, OrderStatus.Confirmed };

    private readonly MongoDbContext _db;

    public OrderService(MongoDbContext db)
    {
        _db = db;
    }

    public async Task<List<OrderDto>> GetHistoryAsync(string userId)
    {
        var orders = await _db.Orders.Find(o => o.UserId == userId).SortByDescending(o => o.CreatedAt).ToListAsync();
        return orders.Select(OrderMapper.ToDto).ToList();
    }

    public async Task<OrderDto> GetByIdAsync(string userId, string orderId, bool isAdmin)
    {
        var order = isAdmin
            ? await _db.Orders.Find(o => o.Id == orderId).FirstOrDefaultAsync()
            : await _db.Orders.Find(o => o.Id == orderId && o.UserId == userId).FirstOrDefaultAsync();

        if (order is null) throw new AppException("Order not found", 404);
        return OrderMapper.ToDto(order);
    }

    public async Task<OrderDto> CancelAsync(string userId, string orderId, CancelOrderRequest request)
    {
        var order = await _db.Orders.Find(o => o.Id == orderId && o.UserId == userId).FirstOrDefaultAsync()
            ?? throw new AppException("Order not found", 404);

        if (!CancellableStatuses.Contains(order.Status))
        {
            throw new AppException($"Order in status '{order.Status}' can no longer be cancelled");
        }

        order.Status = OrderStatus.Cancelled;
        order.TrackingHistory.Add(new TrackingEvent { Status = OrderStatus.Cancelled, Note = request.Reason ?? "Cancelled by customer" });
        await _db.Orders.ReplaceOneAsync(o => o.Id == order.Id, order);

        await RestockAsync(order);

        return OrderMapper.ToDto(order);
    }

    public async Task<OrderDto> RequestReturnAsync(string userId, string orderId, ReturnOrderRequest request)
    {
        var order = await _db.Orders.Find(o => o.Id == orderId && o.UserId == userId).FirstOrDefaultAsync()
            ?? throw new AppException("Order not found", 404);

        if (order.Status != OrderStatus.Delivered)
        {
            throw new AppException("Only delivered orders can be returned");
        }

        order.Status = OrderStatus.ReturnRequested;
        order.TrackingHistory.Add(new TrackingEvent { Status = OrderStatus.ReturnRequested, Note = request.Reason ?? "Return requested by customer" });
        await _db.Orders.ReplaceOneAsync(o => o.Id == order.Id, order);

        return OrderMapper.ToDto(order);
    }

    public async Task<PagedResult<OrderDto>> AdminListAsync(int page, int pageSize, string? status)
    {
        var builder = Builders<Order>.Filter;
        var filter = builder.Empty;
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<OrderStatus>(status, true, out var parsed))
        {
            filter = builder.Eq(o => o.Status, parsed);
        }

        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var totalCount = await _db.Orders.CountDocumentsAsync(filter);
        var orders = await _db.Orders.Find(filter).SortByDescending(o => o.CreatedAt).Skip((page - 1) * pageSize).Limit(pageSize).ToListAsync();

        return new PagedResult<OrderDto>
        {
            Items = orders.Select(OrderMapper.ToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<OrderDto> AdminUpdateStatusAsync(string orderId, UpdateOrderStatusRequest request)
    {
        if (!Enum.TryParse<OrderStatus>(request.Status, true, out var newStatus))
        {
            throw new AppException("Invalid order status");
        }

        var order = await _db.Orders.Find(o => o.Id == orderId).FirstOrDefaultAsync()
            ?? throw new AppException("Order not found", 404);

        var wasCancelledOrReturned = order.Status is OrderStatus.Cancelled or OrderStatus.Returned;

        order.Status = newStatus;
        order.TrackingHistory.Add(new TrackingEvent { Status = newStatus, Note = request.Note });
        await _db.Orders.ReplaceOneAsync(o => o.Id == order.Id, order);

        if (!wasCancelledOrReturned && newStatus is OrderStatus.Cancelled or OrderStatus.Returned)
        {
            await RestockAsync(order);
        }

        return OrderMapper.ToDto(order);
    }

    private async Task RestockAsync(Order order)
    {
        foreach (var item in order.Items)
        {
            var product = await _db.Products.Find(p => p.Id == item.ProductId).FirstOrDefaultAsync();
            if (product is null) continue;

            var variant = product.Variants.FirstOrDefault(v => v.Size == item.Size);
            if (variant is not null)
            {
                variant.Stock += item.Quantity;
                await _db.Products.ReplaceOneAsync(p => p.Id == product.Id, product);
            }
        }
    }
}

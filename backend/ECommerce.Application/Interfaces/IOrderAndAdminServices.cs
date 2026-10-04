using ECommerce.Application.Common;
using ECommerce.Application.Dtos;

namespace ECommerce.Application.Interfaces;

public interface ICheckoutService
{
    Task<OrderDto> PlaceOrderAsync(string userId, CheckoutRequest request);
}

public interface IOrderService
{
    Task<List<OrderDto>> GetHistoryAsync(string userId);
    Task<OrderDto> GetByIdAsync(string userId, string orderId, bool isAdmin);
    Task<OrderDto> CancelAsync(string userId, string orderId, CancelOrderRequest request);
    Task<OrderDto> RequestReturnAsync(string userId, string orderId, ReturnOrderRequest request);

    Task<PagedResult<OrderDto>> AdminListAsync(int page, int pageSize, string? status);
    Task<OrderDto> AdminUpdateStatusAsync(string orderId, UpdateOrderStatusRequest request);
}

public interface IAdminUserService
{
    Task<PagedResult<AdminUserDto>> SearchAsync(string? search, int page, int pageSize);
    Task SetActiveAsync(string userId, bool isActive);
}

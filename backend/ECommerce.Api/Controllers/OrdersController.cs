using ECommerce.Api.Common;
using ECommerce.Application.Common;
using ECommerce.Application.Dtos;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<ActionResult<List<OrderDto>>> GetHistory() => Ok(await _orderService.GetHistoryAsync(this.GetUserId()));

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetById(string id) =>
        Ok(await _orderService.GetByIdAsync(this.GetUserId(), id, User.IsInRole("Admin")));

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<OrderDto>> Cancel(string id, CancelOrderRequest request) =>
        Ok(await _orderService.CancelAsync(this.GetUserId(), id, request));

    [HttpPost("{id}/return")]
    public async Task<ActionResult<OrderDto>> RequestReturn(string id, ReturnOrderRequest request) =>
        Ok(await _orderService.RequestReturnAsync(this.GetUserId(), id, request));

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/all")]
    public async Task<ActionResult<PagedResult<OrderDto>>> AdminList([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? status = null) =>
        Ok(await _orderService.AdminListAsync(page, pageSize, status));

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}/status")]
    public async Task<ActionResult<OrderDto>> AdminUpdateStatus(string id, UpdateOrderStatusRequest request) =>
        Ok(await _orderService.AdminUpdateStatusAsync(id, request));
}

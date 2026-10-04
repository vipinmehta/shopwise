using ECommerce.Api.Common;
using ECommerce.Application.Dtos;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/cart")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<ActionResult<CartDto>> Get() => Ok(await _cartService.GetCartAsync(this.GetUserId()));

    [HttpPost("items")]
    public async Task<ActionResult<CartDto>> AddItem(AddCartItemRequest request) => Ok(await _cartService.AddItemAsync(this.GetUserId(), request));

    [HttpPut("items")]
    public async Task<ActionResult<CartDto>> UpdateItem(UpdateCartItemRequest request) => Ok(await _cartService.UpdateItemAsync(this.GetUserId(), request));

    [HttpDelete("items")]
    public async Task<ActionResult<CartDto>> RemoveItem([FromQuery] string productId, [FromQuery] string? size) =>
        Ok(await _cartService.RemoveItemAsync(this.GetUserId(), productId, size));

    [HttpPost("save-for-later")]
    public async Task<ActionResult<CartDto>> SaveForLater([FromQuery] string productId, [FromQuery] string? size) =>
        Ok(await _cartService.SaveForLaterAsync(this.GetUserId(), productId, size));

    [HttpPost("move-to-cart")]
    public async Task<ActionResult<CartDto>> MoveToCart([FromQuery] string productId, [FromQuery] string? size) =>
        Ok(await _cartService.MoveToCartAsync(this.GetUserId(), productId, size));

    [HttpPost("wishlist/{productId}")]
    public async Task<ActionResult<CartDto>> ToggleWishlist(string productId) =>
        Ok(await _cartService.ToggleWishlistAsync(this.GetUserId(), productId));
}

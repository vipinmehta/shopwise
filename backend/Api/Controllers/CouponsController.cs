using ECommerce.Application.Dtos;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[ApiController]
[Route("api/coupons")]
public class CouponsController : ControllerBase
{
    private readonly ICouponService _couponService;

    public CouponsController(ICouponService couponService)
    {
        _couponService = couponService;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<List<CouponDto>>> GetAll() => Ok(await _couponService.GetAllAsync());

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<CouponDto>> Create(UpsertCouponRequest request) => Ok(await _couponService.CreateAsync(request));

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<CouponDto>> Update(string id, UpsertCouponRequest request) => Ok(await _couponService.UpdateAsync(id, request));

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _couponService.DeleteAsync(id);
        return NoContent();
    }

    [Authorize]
    [HttpPost("validate")]
    public async Task<ActionResult<ValidateCouponResponse>> Validate(ValidateCouponRequest request) => Ok(await _couponService.ValidateAsync(request));
}

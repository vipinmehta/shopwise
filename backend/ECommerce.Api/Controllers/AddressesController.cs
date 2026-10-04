using ECommerce.Api.Common;
using ECommerce.Application.Dtos;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/addresses")]
public class AddressesController : ControllerBase
{
    private readonly IAddressService _addressService;

    public AddressesController(IAddressService addressService)
    {
        _addressService = addressService;
    }

    [HttpGet]
    public async Task<ActionResult<List<AddressDto>>> GetAll() => Ok(await _addressService.GetAllAsync(this.GetUserId()));

    [HttpPost]
    public async Task<ActionResult<AddressDto>> Create(UpsertAddressRequest request) => Ok(await _addressService.CreateAsync(this.GetUserId(), request));

    [HttpPut("{id}")]
    public async Task<ActionResult<AddressDto>> Update(string id, UpsertAddressRequest request) => Ok(await _addressService.UpdateAsync(this.GetUserId(), id, request));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _addressService.DeleteAsync(this.GetUserId(), id);
        return NoContent();
    }
}

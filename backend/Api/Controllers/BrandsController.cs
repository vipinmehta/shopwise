using ECommerce.Application.Dtos;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[ApiController]
[Route("api/brands")]
public class BrandsController : ControllerBase
{
    private readonly IBrandService _brandService;

    public BrandsController(IBrandService brandService)
    {
        _brandService = brandService;
    }

    [HttpGet]
    public async Task<ActionResult<List<BrandDto>>> GetAll() => Ok(await _brandService.GetAllAsync());

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<BrandDto>> Create(UpsertBrandRequest request) => Ok(await _brandService.CreateAsync(request));

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<BrandDto>> Update(string id, UpsertBrandRequest request) => Ok(await _brandService.UpdateAsync(id, request));

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _brandService.DeleteAsync(id);
        return NoContent();
    }
}

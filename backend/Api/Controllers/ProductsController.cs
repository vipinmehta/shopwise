using ECommerce.Api.Common;
using ECommerce.Application.Common;
using ECommerce.Application.Dtos;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IReviewService _reviewService;

    public ProductsController(IProductService productService, IReviewService reviewService)
    {
        _productService = productService;
        _reviewService = reviewService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductListItemDto>>> Search([FromQuery] ProductQuery query) =>
        Ok(await _productService.SearchAsync(query));

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDetailDto>> GetById(string id) => Ok(await _productService.GetByIdAsync(id));

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ProductDetailDto>> Create(UpsertProductRequest request) => Ok(await _productService.CreateAsync(request));

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<ProductDetailDto>> Update(string id, UpsertProductRequest request) => Ok(await _productService.UpdateAsync(id, request));

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _productService.DeleteAsync(id);
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id}/stock")]
    public async Task<IActionResult> AdjustStock(string id, StockAdjustmentRequest request)
    {
        await _productService.AdjustStockAsync(id, request.Size, request.Delta);
        return Ok(new { message = "Stock updated" });
    }

    [HttpGet("{id}/reviews")]
    public async Task<ActionResult<List<ReviewDto>>> GetReviews(string id) => Ok(await _reviewService.GetForProductAsync(id));

    [Authorize]
    [HttpPost("{id}/reviews")]
    public async Task<ActionResult<ReviewDto>> AddReview(string id, CreateReviewRequest request) =>
        Ok(await _reviewService.AddAsync(id, this.GetUserId(), this.GetUserName(), request));
}

using ECommerce.Application.Common;
using ECommerce.Application.Dtos;

namespace ECommerce.Application.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync();
    Task<CategoryDto> CreateAsync(UpsertCategoryRequest request);
    Task<CategoryDto> UpdateAsync(string id, UpsertCategoryRequest request);
    Task DeleteAsync(string id);
}

public interface IBrandService
{
    Task<List<BrandDto>> GetAllAsync();
    Task<BrandDto> CreateAsync(UpsertBrandRequest request);
    Task<BrandDto> UpdateAsync(string id, UpsertBrandRequest request);
    Task DeleteAsync(string id);
}

public interface IProductService
{
    Task<PagedResult<ProductListItemDto>> SearchAsync(ProductQuery query);
    Task<ProductDetailDto> GetByIdAsync(string id);
    Task<ProductDetailDto> CreateAsync(UpsertProductRequest request);
    Task<ProductDetailDto> UpdateAsync(string id, UpsertProductRequest request);
    Task DeleteAsync(string id);
    Task AdjustStockAsync(string id, string size, int delta);
}

public interface IReviewService
{
    Task<List<ReviewDto>> GetForProductAsync(string productId);
    Task<ReviewDto> AddAsync(string productId, string userId, string userName, CreateReviewRequest request);
}

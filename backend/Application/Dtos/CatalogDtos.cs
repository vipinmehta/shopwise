namespace ECommerce.Application.Dtos;

public record CategoryDto(string Id, string Name, string Slug, string? Image, string? ParentId, bool IsActive);
public record UpsertCategoryRequest(string Name, string Slug, string? Image, string? ParentId, bool IsActive);

public record BrandDto(string Id, string Name, string Slug, string? Logo, bool IsActive);
public record UpsertBrandRequest(string Name, string Slug, string? Logo, bool IsActive);

public record ProductVariantDto(string Size, string? Color, string Sku, int Stock);
public record SizeChartRowDto(string Size, Dictionary<string, string> Measurements);

public record ProductListItemDto(
    string Id, string Name, string Slug, decimal Price, decimal? DiscountPrice,
    string? Thumbnail, double AverageRating, int ReviewCount, bool InStock);

public record ProductDetailDto(
    string Id, string Name, string Slug, string Description,
    string CategoryId, string BrandId, decimal Price, decimal? DiscountPrice,
    List<string> Images, List<string> Videos, List<ProductVariantDto> Variants,
    List<SizeChartRowDto> SizeChart, List<string> Tags,
    double AverageRating, int ReviewCount, bool IsActive);

public record UpsertProductRequest(
    string Name, string Slug, string Description, string CategoryId, string BrandId,
    decimal Price, decimal? DiscountPrice, List<string> Images, List<string> Videos,
    List<ProductVariantDto> Variants, List<SizeChartRowDto> SizeChart, List<string> Tags, bool IsActive);

public record ProductQuery(
    string? Search, string? CategoryId, string? BrandId, decimal? MinPrice, decimal? MaxPrice,
    string? Sort, int Page = 1, int PageSize = 12);

public record ReviewDto(string Id, string UserName, int Rating, string? Comment, DateTime CreatedAt);
public record CreateReviewRequest(int Rating, string? Comment);

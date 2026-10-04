using ECommerce.Application.Common;
using ECommerce.Application.Dtos;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using MongoDB.Driver;

namespace ECommerce.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly MongoDbContext _db;

    public ProductService(MongoDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<ProductListItemDto>> SearchAsync(ProductQuery query)
    {
        var builder = Builders<Product>.Filter;
        var filter = builder.Eq(p => p.IsActive, true);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            filter &= builder.Regex(p => p.Name, new MongoDB.Bson.BsonRegularExpression(query.Search, "i"));
        }
        if (!string.IsNullOrWhiteSpace(query.CategoryId))
        {
            var categoryIds = await _db.Categories.Find(c => c.ParentId == query.CategoryId).Project(c => c.Id).ToListAsync();
            categoryIds.Add(query.CategoryId);
            filter &= builder.In(p => p.CategoryId, categoryIds);
        }
        if (!string.IsNullOrWhiteSpace(query.BrandId))
        {
            filter &= builder.Eq(p => p.BrandId, query.BrandId);
        }
        if (query.MinPrice.HasValue)
        {
            filter &= builder.Gte(p => p.Price, query.MinPrice.Value);
        }
        if (query.MaxPrice.HasValue)
        {
            filter &= builder.Lte(p => p.Price, query.MaxPrice.Value);
        }

        var sort = query.Sort switch
        {
            "price_asc" => Builders<Product>.Sort.Ascending(p => p.Price),
            "price_desc" => Builders<Product>.Sort.Descending(p => p.Price),
            "rating" => Builders<Product>.Sort.Descending(p => p.AverageRating),
            "newest" => Builders<Product>.Sort.Descending(p => p.CreatedAt),
            _ => Builders<Product>.Sort.Descending(p => p.CreatedAt)
        };

        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 12 : query.PageSize;

        var totalCount = await _db.Products.CountDocumentsAsync(filter);
        var products = await _db.Products.Find(filter).Sort(sort).Skip((page - 1) * pageSize).Limit(pageSize).ToListAsync();

        return new PagedResult<ProductListItemDto>
        {
            Items = products.Select(ToListItemDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ProductDetailDto> GetByIdAsync(string id)
    {
        var product = await _db.Products.Find(p => p.Id == id).FirstOrDefaultAsync()
            ?? throw new AppException("Product not found", 404);
        return ToDetailDto(product);
    }

    public async Task<ProductDetailDto> CreateAsync(UpsertProductRequest request)
    {
        var product = new Product
        {
            Name = request.Name,
            Slug = request.Slug,
            Description = request.Description,
            CategoryId = request.CategoryId,
            BrandId = request.BrandId,
            Price = request.Price,
            DiscountPrice = request.DiscountPrice,
            Images = request.Images,
            Videos = request.Videos,
            Variants = request.Variants.Select(v => new ProductVariant { Size = v.Size, Color = v.Color, Sku = v.Sku, Stock = v.Stock }).ToList(),
            SizeChart = request.SizeChart.Select(s => new SizeChartRow { Size = s.Size, Measurements = s.Measurements }).ToList(),
            Tags = request.Tags,
            IsActive = request.IsActive
        };
        await _db.Products.InsertOneAsync(product);
        return ToDetailDto(product);
    }

    public async Task<ProductDetailDto> UpdateAsync(string id, UpsertProductRequest request)
    {
        var product = await _db.Products.Find(p => p.Id == id).FirstOrDefaultAsync()
            ?? throw new AppException("Product not found", 404);

        product.Name = request.Name;
        product.Slug = request.Slug;
        product.Description = request.Description;
        product.CategoryId = request.CategoryId;
        product.BrandId = request.BrandId;
        product.Price = request.Price;
        product.DiscountPrice = request.DiscountPrice;
        product.Images = request.Images;
        product.Videos = request.Videos;
        product.Variants = request.Variants.Select(v => new ProductVariant { Size = v.Size, Color = v.Color, Sku = v.Sku, Stock = v.Stock }).ToList();
        product.SizeChart = request.SizeChart.Select(s => new SizeChartRow { Size = s.Size, Measurements = s.Measurements }).ToList();
        product.Tags = request.Tags;
        product.IsActive = request.IsActive;

        await _db.Products.ReplaceOneAsync(p => p.Id == id, product);
        return ToDetailDto(product);
    }

    public async Task DeleteAsync(string id)
    {
        await _db.Products.DeleteOneAsync(p => p.Id == id);
    }

    public async Task AdjustStockAsync(string id, string size, int delta)
    {
        var product = await _db.Products.Find(p => p.Id == id).FirstOrDefaultAsync()
            ?? throw new AppException("Product not found", 404);

        var variant = product.Variants.FirstOrDefault(v => v.Size == size)
            ?? throw new AppException("Variant not found", 404);

        variant.Stock = Math.Max(0, variant.Stock + delta);
        await _db.Products.ReplaceOneAsync(p => p.Id == id, product);
    }

    private static ProductListItemDto ToListItemDto(Product p) => new(
        p.Id, p.Name, p.Slug, p.Price, p.DiscountPrice,
        p.Images.FirstOrDefault(), p.AverageRating, p.ReviewCount, p.TotalStock > 0);

    private static ProductDetailDto ToDetailDto(Product p) => new(
        p.Id, p.Name, p.Slug, p.Description, p.CategoryId, p.BrandId, p.Price, p.DiscountPrice,
        p.Images, p.Videos,
        p.Variants.Select(v => new ProductVariantDto(v.Size, v.Color, v.Sku, v.Stock)).ToList(),
        p.SizeChart.Select(s => new SizeChartRowDto(s.Size, s.Measurements)).ToList(),
        p.Tags, p.AverageRating, p.ReviewCount, p.IsActive);
}

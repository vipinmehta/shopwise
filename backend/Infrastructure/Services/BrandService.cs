using ECommerce.Application.Common;
using ECommerce.Application.Dtos;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using MongoDB.Driver;

namespace ECommerce.Infrastructure.Services;

public class BrandService : IBrandService
{
    private readonly MongoDbContext _db;

    public BrandService(MongoDbContext db)
    {
        _db = db;
    }

    public async Task<List<BrandDto>> GetAllAsync()
    {
        var brands = await _db.Brands.Find(FilterDefinition<Brand>.Empty).ToListAsync();
        return brands.Select(ToDto).ToList();
    }

    public async Task<BrandDto> CreateAsync(UpsertBrandRequest request)
    {
        var brand = new Brand { Name = request.Name, Slug = request.Slug, Logo = request.Logo, IsActive = request.IsActive };
        await _db.Brands.InsertOneAsync(brand);
        return ToDto(brand);
    }

    public async Task<BrandDto> UpdateAsync(string id, UpsertBrandRequest request)
    {
        var update = Builders<Brand>.Update
            .Set(b => b.Name, request.Name)
            .Set(b => b.Slug, request.Slug)
            .Set(b => b.Logo, request.Logo)
            .Set(b => b.IsActive, request.IsActive);

        var brand = await _db.Brands.FindOneAndUpdateAsync(
            b => b.Id == id, update,
            new FindOneAndUpdateOptions<Brand> { ReturnDocument = ReturnDocument.After });

        if (brand is null) throw new AppException("Brand not found", 404);
        return ToDto(brand);
    }

    public async Task DeleteAsync(string id)
    {
        await _db.Brands.DeleteOneAsync(b => b.Id == id);
    }

    private static BrandDto ToDto(Brand b) => new(b.Id, b.Name, b.Slug, b.Logo, b.IsActive);
}

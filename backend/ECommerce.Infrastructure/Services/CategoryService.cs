using ECommerce.Application.Common;
using ECommerce.Application.Dtos;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using MongoDB.Driver;

namespace ECommerce.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly MongoDbContext _db;

    public CategoryService(MongoDbContext db)
    {
        _db = db;
    }

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        var categories = await _db.Categories.Find(FilterDefinition<Category>.Empty).ToListAsync();
        return categories.Select(ToDto).ToList();
    }

    public async Task<CategoryDto> CreateAsync(UpsertCategoryRequest request)
    {
        var category = new Category
        {
            Name = request.Name,
            Slug = request.Slug,
            Image = request.Image,
            ParentId = request.ParentId,
            IsActive = request.IsActive
        };
        await _db.Categories.InsertOneAsync(category);
        return ToDto(category);
    }

    public async Task<CategoryDto> UpdateAsync(string id, UpsertCategoryRequest request)
    {
        var update = Builders<Category>.Update
            .Set(c => c.Name, request.Name)
            .Set(c => c.Slug, request.Slug)
            .Set(c => c.Image, request.Image)
            .Set(c => c.ParentId, request.ParentId)
            .Set(c => c.IsActive, request.IsActive);

        var category = await _db.Categories.FindOneAndUpdateAsync(
            c => c.Id == id, update,
            new FindOneAndUpdateOptions<Category> { ReturnDocument = ReturnDocument.After });

        if (category is null) throw new AppException("Category not found", 404);
        return ToDto(category);
    }

    public async Task DeleteAsync(string id)
    {
        await _db.Categories.DeleteOneAsync(c => c.Id == id);
    }

    private static CategoryDto ToDto(Category c) => new(c.Id, c.Name, c.Slug, c.Image, c.ParentId, c.IsActive);
}

using ECommerce.Application.Common;
using ECommerce.Application.Dtos;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ECommerce.Infrastructure.Services;

public class AdminUserService : IAdminUserService
{
    private readonly MongoDbContext _db;

    public AdminUserService(MongoDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<AdminUserDto>> SearchAsync(string? search, int page, int pageSize)
    {
        var builder = Builders<User>.Filter;
        var filter = builder.Empty;
        if (!string.IsNullOrWhiteSpace(search))
        {
            var regex = new BsonRegularExpression(search, "i");
            filter = builder.Or(builder.Regex(u => u.Name, regex), builder.Regex(u => u.Email, regex));
        }

        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var totalCount = await _db.Users.CountDocumentsAsync(filter);
        var users = await _db.Users.Find(filter).SortByDescending(u => u.CreatedAt).Skip((page - 1) * pageSize).Limit(pageSize).ToListAsync();

        return new PagedResult<AdminUserDto>
        {
            Items = users.Select(u => new AdminUserDto(u.Id, u.Name, u.Email, u.Phone, u.Role.ToString(), u.IsActive, u.CreatedAt)).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task SetActiveAsync(string userId, bool isActive)
    {
        var result = await _db.Users.UpdateOneAsync(u => u.Id == userId, Builders<User>.Update.Set(u => u.IsActive, isActive));
        if (result.MatchedCount == 0)
        {
            throw new AppException("User not found", 404);
        }
    }
}

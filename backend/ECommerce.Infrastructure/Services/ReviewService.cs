using ECommerce.Application.Common;
using ECommerce.Application.Dtos;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using MongoDB.Driver;

namespace ECommerce.Infrastructure.Services;

public class ReviewService : IReviewService
{
    private readonly MongoDbContext _db;

    public ReviewService(MongoDbContext db)
    {
        _db = db;
    }

    public async Task<List<ReviewDto>> GetForProductAsync(string productId)
    {
        var reviews = await _db.Reviews.Find(r => r.ProductId == productId).SortByDescending(r => r.CreatedAt).ToListAsync();
        return reviews.Select(r => new ReviewDto(r.Id, r.UserName, r.Rating, r.Comment, r.CreatedAt)).ToList();
    }

    public async Task<ReviewDto> AddAsync(string productId, string userId, string userName, CreateReviewRequest request)
    {
        if (request.Rating is < 1 or > 5)
        {
            throw new AppException("Rating must be between 1 and 5");
        }

        var product = await _db.Products.Find(p => p.Id == productId).FirstOrDefaultAsync()
            ?? throw new AppException("Product not found", 404);

        var review = new Review
        {
            ProductId = productId,
            UserId = userId,
            UserName = userName,
            Rating = request.Rating,
            Comment = request.Comment
        };
        await _db.Reviews.InsertOneAsync(review);

        var newReviewCount = product.ReviewCount + 1;
        var newAverage = ((product.AverageRating * product.ReviewCount) + request.Rating) / newReviewCount;
        await _db.Products.UpdateOneAsync(p => p.Id == productId,
            Builders<Product>.Update.Set(p => p.ReviewCount, newReviewCount).Set(p => p.AverageRating, Math.Round(newAverage, 2)));

        return new ReviewDto(review.Id, review.UserName, review.Rating, review.Comment, review.CreatedAt);
    }
}

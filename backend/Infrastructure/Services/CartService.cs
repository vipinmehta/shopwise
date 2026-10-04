using ECommerce.Application.Common;
using ECommerce.Application.Dtos;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using MongoDB.Driver;

namespace ECommerce.Infrastructure.Services;

public class CartService : ICartService
{
    private readonly MongoDbContext _db;

    public CartService(MongoDbContext db)
    {
        _db = db;
    }

    public async Task<CartDto> GetCartAsync(string userId)
    {
        var cart = await GetOrCreateCartAsync(userId);
        return await ToDtoAsync(cart);
    }

    public async Task<CartDto> AddItemAsync(string userId, AddCartItemRequest request)
    {
        var cart = await GetOrCreateCartAsync(userId);
        var product = await _db.Products.Find(p => p.Id == request.ProductId).FirstOrDefaultAsync()
            ?? throw new AppException("Product not found", 404);

        var existing = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId && i.Size == request.Size);
        if (existing is not null)
        {
            existing.Quantity += request.Quantity;
        }
        else
        {
            cart.Items.Add(new CartItem { ProductId = request.ProductId, Size = request.Size, Quantity = request.Quantity });
        }

        await _db.Carts.ReplaceOneAsync(c => c.Id == cart.Id, cart);
        return await ToDtoAsync(cart);
    }

    public async Task<CartDto> UpdateItemAsync(string userId, UpdateCartItemRequest request)
    {
        var cart = await GetOrCreateCartAsync(userId);
        var item = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId && i.Size == request.Size)
            ?? throw new AppException("Item not in cart", 404);

        if (request.Quantity <= 0)
        {
            cart.Items.Remove(item);
        }
        else
        {
            item.Quantity = request.Quantity;
        }

        await _db.Carts.ReplaceOneAsync(c => c.Id == cart.Id, cart);
        return await ToDtoAsync(cart);
    }

    public async Task<CartDto> RemoveItemAsync(string userId, string productId, string? size)
    {
        var cart = await GetOrCreateCartAsync(userId);
        cart.Items.RemoveAll(i => i.ProductId == productId && i.Size == size);
        await _db.Carts.ReplaceOneAsync(c => c.Id == cart.Id, cart);
        return await ToDtoAsync(cart);
    }

    public async Task<CartDto> SaveForLaterAsync(string userId, string productId, string? size)
    {
        var cart = await GetOrCreateCartAsync(userId);
        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId && i.Size == size);
        if (item is not null)
        {
            cart.Items.Remove(item);
            cart.SavedForLater.Add(item);
            await _db.Carts.ReplaceOneAsync(c => c.Id == cart.Id, cart);
        }
        return await ToDtoAsync(cart);
    }

    public async Task<CartDto> MoveToCartAsync(string userId, string productId, string? size)
    {
        var cart = await GetOrCreateCartAsync(userId);
        var item = cart.SavedForLater.FirstOrDefault(i => i.ProductId == productId && i.Size == size);
        if (item is not null)
        {
            cart.SavedForLater.Remove(item);
            cart.Items.Add(item);
            await _db.Carts.ReplaceOneAsync(c => c.Id == cart.Id, cart);
        }
        return await ToDtoAsync(cart);
    }

    public async Task<CartDto> ToggleWishlistAsync(string userId, string productId)
    {
        var cart = await GetOrCreateCartAsync(userId);
        if (!await _db.Products.Find(p => p.Id == productId).AnyAsync())
        {
            throw new AppException("Product not found", 404);
        }

        if (cart.WishlistProductIds.Contains(productId))
        {
            cart.WishlistProductIds.Remove(productId);
        }
        else
        {
            cart.WishlistProductIds.Add(productId);
        }

        await _db.Carts.ReplaceOneAsync(c => c.Id == cart.Id, cart);
        return await ToDtoAsync(cart);
    }

    private async Task<Cart> GetOrCreateCartAsync(string userId)
    {
        var cart = await _db.Carts.Find(c => c.UserId == userId).FirstOrDefaultAsync();
        if (cart is null)
        {
            cart = new Cart { UserId = userId };
            await _db.Carts.InsertOneAsync(cart);
        }
        return cart;
    }

    private async Task<CartDto> ToDtoAsync(Cart cart)
    {
        var productIds = cart.Items.Concat(cart.SavedForLater).Select(i => i.ProductId).Distinct().ToList();
        var products = productIds.Count == 0
            ? new List<Product>()
            : await _db.Products.Find(p => productIds.Contains(p.Id)).ToListAsync();
        var productMap = products.ToDictionary(p => p.Id);

        List<CartItemDto> MapItems(List<CartItem> items) => items.Select(i =>
        {
            productMap.TryGetValue(i.ProductId, out var product);
            var price = product?.DiscountPrice ?? product?.Price ?? 0;
            var stock = product?.Variants.FirstOrDefault(v => v.Size == i.Size)?.Stock ?? product?.TotalStock ?? 0;
            return new CartItemDto(i.ProductId, product?.Name ?? "Unknown product", product?.Images.FirstOrDefault(), i.Size, i.Quantity, price, stock);
        }).ToList();

        var itemDtos = MapItems(cart.Items);
        var subtotal = itemDtos.Sum(i => i.UnitPrice * i.Quantity);

        return new CartDto(itemDtos, MapItems(cart.SavedForLater), cart.WishlistProductIds, subtotal);
    }
}

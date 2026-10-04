using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ECommerce.Domain.Entities;

public class ProductVariant
{
    public string Size { get; set; } = string.Empty;
    public string? Color { get; set; }
    public string Sku { get; set; } = string.Empty;
    public int Stock { get; set; }
}

public class SizeChartRow
{
    public string Size { get; set; } = string.Empty;
    public Dictionary<string, string> Measurements { get; set; } = new();
}

public class Product
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.ObjectId)]
    public string CategoryId { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.ObjectId)]
    public string BrandId { get; set; } = string.Empty;

    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }

    public List<string> Images { get; set; } = new();
    public List<string> Videos { get; set; } = new();
    public List<ProductVariant> Variants { get; set; } = new();
    public List<SizeChartRow> SizeChart { get; set; } = new();
    public List<string> Tags { get; set; } = new();

    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonIgnore]
    public int TotalStock => Variants.Sum(v => v.Stock);
}

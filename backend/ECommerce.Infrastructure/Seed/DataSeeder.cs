using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ECommerce.Infrastructure.Seed;

public static class DataSeeder
{
    private record TypeDef(
        string Plural, string Slug, string Kind, string[] MenSizes, string[] WomenSizes,
        int MinPrice, int MaxPrice, string[] BrandNames, string[] Materials);

    private static readonly TypeDef[] Types =
    {
        new("T-Shirts", "t-shirts", "tee", new[] { "S", "M", "L", "XL", "XXL" }, new[] { "XS", "S", "M", "L", "XL" }, 399, 1499,
            new[] { "UrbanThread", "Acme", "Aurora", "Northline" },
            new[] { "100% combed cotton", "a soft cotton-modal blend", "breathable cotton jersey", "a premium cotton-elastane blend" }),
        new("Jeans", "jeans", "jeans", new[] { "28", "30", "32", "34", "36" }, new[] { "26", "28", "30", "32", "34" }, 1499, 3499,
            new[] { "Denimist", "Northline", "Acme", "UrbanThread" },
            new[] { "stretch denim", "rigid 100% cotton denim", "soft-wash comfort denim", "recycled-cotton denim" }),
        new("Shoes", "shoes", "shoe", new[] { "6", "7", "8", "9", "10", "11" }, new[] { "3", "4", "5", "6", "7", "8" }, 1999, 5999,
            new[] { "StrideWorks", "Aurora", "Northline", "Acme" },
            new[] { "full-grain leather", "soft suede", "vegan leather", "breathable canvas with a leather lining" }),
        new("Sneakers", "sneakers", "sneaker", new[] { "6", "7", "8", "9", "10", "11" }, new[] { "3", "4", "5", "6", "7", "8" }, 2499, 6999,
            new[] { "Kickstart", "PeakRun", "StrideWorks", "Acme" },
            new[] { "breathable mesh", "engineered knit", "premium leather and mesh", "recycled-fibre knit" })
    };

    private static readonly string[] BrandNames = { "Acme", "Northline", "UrbanThread", "StrideWorks", "Denimist", "Kickstart", "Aurora", "PeakRun" };
    private static readonly string[] ReviewerNames = { "Aarav S.", "Meera K.", "Rohan P.", "Ananya D.", "Vikram T.", "Sneha R.", "Kabir M.", "Isha B.", "Nikhil J.", "Priya N." };
    private static readonly string[] PositiveComments =
    {
        "Great quality for the price. Fits true to size.", "Really comfortable, I wear it all the time.", "Looks even better in person.",
        "Fabric feels premium and the stitching is solid.", "Exactly as described, fast delivery.", "Perfect fit, would buy again.",
        "Good value, got lots of compliments.", "Colour is just as shown. Very happy."
    };
    private static readonly string[] MixedComments =
    {
        "Decent product, but I'd size up.", "Okay overall, the colour is slightly different from the photos.", "Good, though it took a few washes to soften up."
    };

    public static async Task SeedAsync(MongoDbContext db, IOptions<SeedSettings> seedOptions)
    {
        var seed = seedOptions.Value;

        if (!await db.Users.Find(u => u.Email == seed.AdminEmail).AnyAsync())
        {
            await db.Users.InsertOneAsync(new User
            {
                Name = "Admin",
                Email = seed.AdminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(seed.AdminPassword),
                Role = UserRole.Admin,
                EmailVerified = true
            });
        }

        if (seed.ResetCatalog)
        {
            await db.Products.DeleteManyAsync(FilterDefinition<Product>.Empty);
            await db.Reviews.DeleteManyAsync(FilterDefinition<Review>.Empty);
            await db.Categories.DeleteManyAsync(FilterDefinition<Category>.Empty);
        }

        await SeedCatalogAsync(db);
        await SeedCouponsAsync(db);
    }

    private static async Task SeedCouponsAsync(MongoDbContext db)
    {
        var coupons = new[]
        {
            new Coupon { Code = "WELCOME10", Type = CouponType.Percentage, Value = 10, MinOrderValue = 500 },
            new Coupon { Code = "FLAT200", Type = CouponType.Flat, Value = 200, MinOrderValue = 1500 },
            new Coupon { Code = "SAVE20", Type = CouponType.Percentage, Value = 20, MinOrderValue = 3000 }
        };
        foreach (var coupon in coupons)
        {
            if (!await db.Coupons.Find(c => c.Code == coupon.Code).AnyAsync())
            {
                await db.Coupons.InsertOneAsync(coupon);
            }
        }
    }

    private static async Task SeedCatalogAsync(MongoDbContext db)
    {
        if (await db.Categories.Find(c => c.Slug == "men").AnyAsync())
        {
            return;
        }

        // Replace the original 3-product starter catalog.
        var oldSlugs = new[] { "classic-cotton-t-shirt", "slim-fit-denim-jeans", "running-sneakers" };
        var oldIds = await db.Products.Find(p => oldSlugs.Contains(p.Slug)).Project(p => p.Id).ToListAsync();
        if (oldIds.Count > 0)
        {
            await db.Reviews.DeleteManyAsync(r => oldIds.Contains(r.ProductId));
            await db.Products.DeleteManyAsync(p => oldIds.Contains(p.Id));
        }
        await db.Categories.DeleteManyAsync(c => c.Slug == "apparel" || c.Slug == "footwear");

        var brands = new Dictionary<string, Brand>();
        foreach (var name in BrandNames)
        {
            var slug = name.ToLowerInvariant();
            var brand = await db.Brands.Find(b => b.Slug == slug).FirstOrDefaultAsync();
            if (brand is null)
            {
                brand = new Brand { Name = name, Slug = slug };
                await db.Brands.InsertOneAsync(brand);
            }
            brands[name] = brand;
        }

        var rnd = new Random(42);
        var reviewerIds = Enumerable.Range(0, ReviewerNames.Length).Select(_ => ObjectId.GenerateNewId().ToString()).ToArray();
        var products = new List<Product>();
        var reviews = new List<Review>();
        var counter = 0;
        var usedNames = new HashSet<string>();

        foreach (var gender in new[] { "Men", "Women" })
        {
            var isMen = gender == "Men";
            var parent = new Category { Name = gender, Slug = gender.ToLowerInvariant() };
            await db.Categories.InsertOneAsync(parent);

            foreach (var type in Types)
            {
                var category = new Category
                {
                    Name = $"{gender}'s {type.Plural}",
                    Slug = $"{gender.ToLowerInvariant()}-{type.Slug}",
                    ParentId = parent.Id
                };
                await db.Categories.InsertOneAsync(category);

                var sizes = isMen ? type.MenSizes : type.WomenSizes;
                var photos = PhotoCatalog.Photos[$"{gender.ToLowerInvariant()}:{(type.Kind == "tee" ? "tee" : type.Slug)}"];

                for (var i = 0; i < photos.Length; i++)
                {
                    var parts = photos[i].Split('|');
                    var photoId = parts[0];
                    var colour = char.ToUpperInvariant(parts[1][0]) + parts[1][1..];
                    var style = parts[2];
                    var brand = brands[type.BrandNames[i % type.BrandNames.Length]];
                    for (var shift = 1; shift < type.BrandNames.Length && usedNames.Contains($"{brand.Name}|{gender}|{style}|{colour}"); shift++)
                    {
                        brand = brands[type.BrandNames[(i + shift) % type.BrandNames.Length]];
                    }
                    usedNames.Add($"{brand.Name}|{gender}|{style}|{colour}");

                    var price = Round99(type.MinPrice + rnd.NextDouble() * (type.MaxPrice - type.MinPrice));
                    decimal? discount = rnd.Next(100) < 40 ? Round99((double)price * (0.7 + rnd.NextDouble() * 0.2)) : null;
                    var codePrefix = $"{type.Slug[..2].ToUpperInvariant()}{(isMen ? "M" : "W")}{i + 1:00}";

                    var product = new Product
                    {
                        Name = $"{brand.Name} {gender}'s {style} - {colour}",
                        Slug = $"{brand.Slug}-{(isMen ? "men" : "women")}-{Slugify(style)}-{Slugify(colour)}-{i + 1}",
                        Description = $"A {colour.ToLowerInvariant()} {(isMen ? "men's" : "women's")} {style.ToLowerInvariant()} by {brand.Name}, made from {type.Materials[i % type.Materials.Length]}. " +
                                      $"Designed for everyday wear with reinforced stitching and a flattering fit. Available in {sizes.Length} sizes.",
                        CategoryId = category.Id,
                        BrandId = brand.Id,
                        Price = price,
                        DiscountPrice = discount,
                        Images = new List<string> { PhotoCatalog.Url(photoId) },
                        Videos = type.Kind == "sneaker" && i % 4 == 0 ? new List<string> { "https://www.w3schools.com/html/mov_bbb.mp4" } : new List<string>(),
                        Variants = sizes.Select(size => new ProductVariant
                        {
                            Size = size,
                            Color = colour,
                            Sku = $"{codePrefix}-{size}",
                            Stock = rnd.Next(100) < 8 ? 0 : rnd.Next(3, 40)
                        }).ToList(),
                        SizeChart = sizes.Select(size => new SizeChartRow { Size = size, Measurements = Measurements(type.Kind, isMen, size) }).ToList(),
                        Tags = new List<string> { gender.ToLowerInvariant(), type.Slug, colour.ToLowerInvariant(), Slugify(style) },
                        CreatedAt = DateTime.UtcNow.AddMinutes(-counter)
                    };

                    var reviewCount = rnd.Next(2, 6);
                    var ratingSum = 0;
                    for (var r = 0; r < reviewCount; r++)
                    {
                        var rating = new[] { 5, 5, 5, 4, 4, 4, 3, 2 }[rnd.Next(8)];
                        ratingSum += rating;
                        var reviewer = rnd.Next(ReviewerNames.Length);
                        reviews.Add(new Review
                        {
                            ProductId = product.Id,
                            UserId = reviewerIds[reviewer],
                            UserName = ReviewerNames[reviewer],
                            Rating = rating,
                            Comment = rating >= 4 ? PositiveComments[rnd.Next(PositiveComments.Length)] : MixedComments[rnd.Next(MixedComments.Length)],
                            CreatedAt = DateTime.UtcNow.AddDays(-rnd.Next(1, 90))
                        });
                    }
                    product.ReviewCount = reviewCount;
                    product.AverageRating = Math.Round(ratingSum / (double)reviewCount, 2);

                    products.Add(product);
                    counter++;
                }
            }
        }

        await db.Products.InsertManyAsync(products);
        await db.Reviews.InsertManyAsync(reviews);
    }

    private static decimal Round99(double value) => Math.Max(99, (decimal)(Math.Round(value / 100) * 100 - 1));

    private static string Slugify(string s)
    {
        var chars = s.ToLowerInvariant().Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray();
        return string.Join('-', new string(chars).Split('-', StringSplitOptions.RemoveEmptyEntries));
    }

    private static Dictionary<string, string> Measurements(string kind, bool isMen, string size)
    {
        switch (kind)
        {
            case "tee":
                var tees = isMen ? new[] { "S", "M", "L", "XL", "XXL" } : new[] { "XS", "S", "M", "L", "XL" };
                var step = Array.IndexOf(tees, size);
                return isMen
                    ? new() { ["Chest (in)"] = $"{36 + 2 * step}", ["Length (in)"] = $"{27 + step}" }
                    : new() { ["Bust (in)"] = $"{32 + 2 * step}", ["Length (in)"] = $"{22 + step}" };
            case "jeans":
                var waist = int.Parse(size);
                return new() { ["Waist (in)"] = size, ["Hip (in)"] = $"{waist + (isMen ? 8 : 10)}", ["Inseam (in)"] = isMen ? "32" : "30" };
            default:
                var uk = int.Parse(size);
                var eu = uk + (isMen ? 34 : 33);
                var footCm = (isMen ? 24.5 : 22.5) + 0.85 * (uk - (isMen ? 6 : 3));
                return new() { ["EU size"] = $"{eu}", ["Foot length (cm)"] = $"{footCm:0.0}" };
        }
    }
}

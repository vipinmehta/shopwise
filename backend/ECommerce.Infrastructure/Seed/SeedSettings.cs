namespace ECommerce.Infrastructure.Seed;

public class SeedSettings
{
    public string AdminEmail { get; set; } = "admin@example.com";
    public string AdminPassword { get; set; } = "Admin@123";

    /// <summary>One-off dev switch: wipes products, reviews and categories, then re-seeds the demo catalog.</summary>
    public bool ResetCatalog { get; set; }
}

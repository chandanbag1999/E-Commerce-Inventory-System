using EIVMS.Domain.Entities.ProductCatalog;
using EIVMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EIVMS.Infrastructure.Seeders;

public static class ProductCatalogSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Categories.AnyAsync())
        {
            Console.WriteLine("✅ Product Catalog already seeded");
            return;
        }

        var electronics = Category.Create("Electronics", "electronics", null, "Electronic devices and accessories", 1);
        await context.Categories.AddAsync(electronics);
        await context.SaveChangesAsync();

        var smartphones = Category.Create("Smartphones", "smartphones", electronics.Id, "Mobile phones and accessories", 1);
        var laptops = Category.Create("Laptops", "laptops", electronics.Id, "Laptops and notebooks", 2);
        var accessories = Category.Create("Accessories", "accessories", electronics.Id, "Electronic accessories", 3);
        await context.Categories.AddRangeAsync(smartphones, laptops, accessories);
        await context.SaveChangesAsync();

        var beverages = Category.Create("Beverages", "beverages", null, "Drinks and beverages", 2);
        await context.Categories.AddAsync(beverages);
        await context.SaveChangesAsync();

        var sports = Category.Create("Sports", "sports", null, "Sports and fitness equipment", 3);
        await context.Categories.AddAsync(sports);
        await context.SaveChangesAsync();

        var kitchen = Category.Create("Kitchen", "kitchen", null, "Kitchenware and utensils", 4);
        await context.Categories.AddAsync(kitchen);
        await context.SaveChangesAsync();

        var home = Category.Create("Home", "home", null, "Home and living", 5);
        await context.Categories.AddAsync(home);
        await context.SaveChangesAsync();

        var decor = Category.Create("Home Decor", "home-decor", home.Id, "Home decoration items", 1);
        var lighting = Category.Create("Lighting", "lighting", home.Id, "Lights and lamps", 2);
        await context.Categories.AddRangeAsync(decor, lighting);
        await context.SaveChangesAsync();

        var apparel = Category.Create("Apparel", "apparel", null, "Clothing and fashion", 6);
        await context.Categories.AddAsync(apparel);
        await context.SaveChangesAsync();

        var menApparel = Category.Create("Men's Clothing", "mens-clothing", apparel.Id, "Men's clothing", 1);
        var womenApparel = Category.Create("Women's Clothing", "womens-clothing", apparel.Id, "Women's clothing", 2);
        await context.Categories.AddRangeAsync(menApparel, womenApparel);
        await context.SaveChangesAsync();

        var tags = new List<Tag>
        {
            Tag.Create("new-arrival"),
            Tag.Create("best-seller"),
            Tag.Create("featured"),
            Tag.Create("sale"),
            Tag.Create("eco-friendly"),
            Tag.Create("premium")
        };
        await context.Tags.AddRangeAsync(tags);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ Product Catalog seeded successfully!");
        Console.WriteLine($"   Categories: {15}");
        Console.WriteLine($"   Tags: {tags.Count}");
    }
}
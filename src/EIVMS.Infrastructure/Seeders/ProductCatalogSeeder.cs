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

        var fashion = Category.Create("Fashion", "fashion", null, "Fashion and clothing items", 2);
        await context.Categories.AddAsync(fashion);
        await context.SaveChangesAsync();

        var menFashion = Category.Create("Men's Fashion", "mens-fashion", fashion.Id, "Men's clothing and accessories", 1);
        var womenFashion = Category.Create("Women's Fashion", "womens-fashion", fashion.Id, "Women's clothing and accessories", 2);
        await context.Categories.AddRangeAsync(menFashion, womenFashion);
        await context.SaveChangesAsync();

        var homeGarden = Category.Create("Home & Garden", "home-garden", null, "Home and garden products", 3);
        await context.Categories.AddAsync(homeGarden);
        await context.SaveChangesAsync();

        var furniture = Category.Create("Furniture", "furniture", homeGarden.Id, "Home furniture", 1);
        var decor = Category.Create("Home Decor", "home-decor", homeGarden.Id, "Home decoration items", 2);
        await context.Categories.AddRangeAsync(furniture, decor);
        await context.SaveChangesAsync();

        var sports = Category.Create("Sports & Outdoors", "sports-outdoors", null, "Sports and outdoor equipment", 4);
        await context.Categories.AddAsync(sports);
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
        Console.WriteLine($"   Categories: 12");
        Console.WriteLine($"   Tags: {tags.Count}");
    }
}
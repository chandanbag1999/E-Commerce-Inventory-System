using EIVMS.Domain.Entities.Inventory;
using EIVMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EIVMS.Infrastructure.Seeders;

public static class WarehouseSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Warehouses.AnyAsync())
        {
            Console.WriteLine("✅ Warehouses already seeded");
            return;
        }

        var warehouses = new List<Warehouse>
        {
            Warehouse.Create(
                "Mumbai Central",
                "MUM-CEN",
                "Dock 15, J Nehru Road",
                "Mumbai",
                "Maharashtra",
                "India",
                "400001",
                19.0760,
                72.8777,
                true),

            Warehouse.Create(
                "Delhi Hub",
                "DEL-HUB",
                "Plot 42, Industrial Area",
                "New Delhi",
                "Delhi",
                "India",
                "110001",
                28.6139,
                77.2090,
                false),

            Warehouse.Create(
                "Bangalore South",
                "BLR-SOU",
                "Electronic City Phase 2",
                "Bangalore",
                "Karnataka",
                "India",
                "560100",
                12.9166,
                77.6100,
                false),

            Warehouse.Create(
                "Chennai Port",
                "MAA-PORT",
                "Port Trust Area",
                "Chennai",
                "Tamil Nadu",
                "India",
                "600001",
                13.0827,
                80.2707,
                false)
        };

        var mumbai = warehouses[0];
        mumbai.UpdateDetails(
            "Mumbai Central",
            "Main distribution center for western India",
            "Dock 15, J Nehru Road",
            "Anita Desai",
            "+91 9876543210",
            "mumbai.central@eivms.com",
            1);
        mumbai.SetCapacity(50000);
        mumbai.SetCapacity(36000);

        var delhi = warehouses[1];
        delhi.UpdateDetails(
            "Delhi Hub",
            "Northern India hub warehouse",
            "Plot 42, Industrial Area",
            "Ramesh Verma",
            "+91 9876543211",
            "delhi.hub@eivms.com",
            2);
        delhi.SetCapacity(75000);
        delhi.SetCapacity(43500);

        var bangalore = warehouses[2];
        bangalore.UpdateDetails(
            "Bangalore South",
            "Southern India distribution center",
            "Electronic City Phase 2",
            "Lakshmi Rao",
            "+91 9876543212",
            "bangalore.south@eivms.com",
            3);
        bangalore.SetCapacity(35000);
        bangalore.SetCapacity(31850);

        var chennai = warehouses[3];
        chennai.UpdateDetails(
            "Chennai Port",
            "Port warehouse - currently under maintenance",
            "Port Trust Area",
            "Unassigned",
            "+91 9876543213",
            "chennai.port@eivms.com",
            4);
        chennai.SetCapacity(40000);
        chennai.SetCapacity(6000);
        chennai.SetUnderMaintenance();

        await context.Warehouses.AddRangeAsync(warehouses);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ Warehouses seeded successfully!");
        Console.WriteLine($"   Warehouses: {warehouses.Count}");
    }
}
using EcommerceInventory.Domain.Entities;
using EcommerceInventory.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EcommerceInventory.Infrastructure.Persistence.Seed;

/// <summary>
/// Seeds the SuperAdmin user with a default password
/// This runs once on first migration to ensure there's always an admin account
/// </summary>
public static class SuperAdminSeeder
{
    // Fixed GUIDs for SuperAdmin user and related records
    private static readonly Guid SuperAdminRoleId = Guid.Parse("11111111-0000-0000-0000-000000000001");
    private static readonly Guid SuperAdminUserId = Guid.Parse("31111111-0000-0000-0000-000000000001");

    /// <summary>
    /// Seeds the SuperAdmin user if it doesn't exist
    /// </summary>
    public static async Task SeedSuperAdminAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("SuperAdminSeeder");
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        // Check if SuperAdmin already exists
        var superAdminExists = await context.Users.AnyAsync(u => u.Id == SuperAdminUserId);
        if (superAdminExists)
        {
            logger.LogInformation("SuperAdmin user already exists, skipping seed.");
            return;
        }

        logger.LogInformation("Seeding SuperAdmin user...");

        // Get settings from configuration
        var superAdminSettings = configuration.GetSection("SuperAdmin");
        var email = superAdminSettings["Email"] ?? "superadmin@ecommerce.local";
        var password = superAdminSettings["Password"] ?? "SuperAdmin@123!";
        var fullName = superAdminSettings["FullName"] ?? "Super Administrator";
        var phone = superAdminSettings["Phone"];

        // Create the SuperAdmin user
        // Note: We need to hash the password using the same method as the application
        // For seeding, we'll use a simple BCrypt hash (or whatever the app uses)
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        var seedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var superAdminUser = new User
        {
            Id = SuperAdminUserId,
            FullName = fullName,
            Email = email.ToLower(),
            PasswordHash = passwordHash,
            Phone = phone,
            Status = UserStatus.Active,
            IsEmailVerified = true, // Auto-verify SuperAdmin
            LastLoginAt = null,
            CreatedAt = seedDate,
            UpdatedAt = seedDate
        };

        context.Users.Add(superAdminUser);

        // Assign SuperAdmin role to the user
        var userRole = new UserRole
        {
            UserId = SuperAdminUserId,
            RoleId = SuperAdminRoleId,
            AssignedAt = seedDate,
            AssignedBy = null
        };

        context.UserRoles.Add(userRole);

        await context.SaveChangesAsync();

        logger.LogInformation("SuperAdmin user '{Email}' created successfully with SuperAdmin role.", email);
    }
}

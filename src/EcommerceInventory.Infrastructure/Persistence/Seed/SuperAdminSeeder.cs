using EcommerceInventory.Domain.Entities;
using EcommerceInventory.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BCrypt.Net;

namespace EcommerceInventory.Infrastructure.Persistence.Seed;

public static class SuperAdminSeeder
{
    public static async Task SeedAsync(
        AppDbContext context,
        IConfiguration configuration,
        ILogger logger)
    {
        try
        {
            var fullName = configuration["SuperAdmin:FullName"]
                           ?? "System Administrator";
            var email    = configuration["SuperAdmin:Email"]
                           ?? "superadmin@system.com";
            var password = configuration["SuperAdmin:Password"]
                           ?? "SuperAdmin@123";
            var phone    = configuration["SuperAdmin:Phone"]
                           ?? "";

            var superAdminRoleId = Guid.Parse(
                "11111111-0000-0000-0000-000000000001");

            // Check if SuperAdmin user already exists
            var existingUser = await context.Users
                .FirstOrDefaultAsync(u => u.Email == email.ToLower().Trim());

            if (existingUser != null)
            {
                logger.LogInformation(
                    "SuperAdmin user already exists: {Email}", email);
                return;
            }

            // Check role exists
            var superAdminRole = await context.Roles
                .FirstOrDefaultAsync(r => r.Id == superAdminRoleId);

            if (superAdminRole == null)
            {
                logger.LogWarning(
                    "SuperAdmin role not found. " +
                    "Make sure migrations have been applied.");
                return;
            }

            // Create user via factory method
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
            var superAdminUser = User.Create(
                fullName, email, passwordHash,
                string.IsNullOrWhiteSpace(phone) ? null : phone);

            // Override the auto-generated Id with fixed seed Id
            superAdminUser.Id = Guid.Parse(
                "33333333-0000-0000-0000-000000000001");

            // Mark email as verified
            superAdminUser.VerifyEmail();

            await context.Users.AddAsync(superAdminUser);
            await context.SaveChangesAsync();

            // Assign SuperAdmin role
            var userRole = new UserRole
            {
                UserId     = superAdminUser.Id,
                RoleId     = superAdminRoleId,
                AssignedAt = DateTime.UtcNow,
                AssignedBy = superAdminUser.Id
            };

            await context.UserRoles.AddAsync(userRole);
            await context.SaveChangesAsync();

            logger.LogInformation(
                "SuperAdmin user seeded successfully. Email: {Email}", email);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding SuperAdmin user.");
            throw;
        }
    }
}

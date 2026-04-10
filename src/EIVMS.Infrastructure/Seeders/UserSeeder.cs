using EIVMS.Domain.Entities.Identity;
using EIVMS.Infrastructure.Persistence;
using EIVMS.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EIVMS.Infrastructure.Seeders;

public static class UserSeeder
{
    public static async Task SeedAsync(AppDbContext context, IConfiguration configuration)
    {
        var adminEmail = configuration["DefaultAdmin:Email"] ?? "admin@nexusops.com";
        var adminPassword = configuration["DefaultAdmin:Password"] ?? "Admin@123";
        var adminRoleName = configuration["DefaultAdmin:Role"] ?? "SuperAdmin";

        var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail.ToLowerInvariant());

        var superAdminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == adminRoleName);
        if (superAdminRole == null)
        {
            Console.WriteLine($"⚠️ {adminRoleName} role not found - run RolePermissionSeeder first");
            return;
        }

        if (existingUser == null)
        {
            var passwordHasher = new PasswordHasher();
            var passwordHash = passwordHasher.HashPassword(adminPassword);

            var nameParts = adminEmail.Split('@')[0].Split('.');
            var firstName = nameParts.Length > 0 ? nameParts[0] : "System";
            var lastName = nameParts.Length > 1 ? nameParts[1] : "Administrator";

            var adminUser = User.Create(firstName, lastName, adminEmail.ToLowerInvariant(), passwordHash);

            await context.Users.AddAsync(adminUser);
            await context.SaveChangesAsync();

            var userRole = UserRole.Create(adminUser.Id, superAdminRole.Id);
            await context.UserRoles.AddAsync(userRole);
            await context.SaveChangesAsync();

            Console.WriteLine($"✅ Default {adminRoleName} user created!");
            Console.WriteLine($"   Email: {adminEmail}");
            Console.WriteLine($"   Password: {adminPassword}");
        }
        else
        {
            var hasSuperAdminRole = await context.UserRoles.AnyAsync(ur => ur.UserId == existingUser.Id && ur.RoleId == superAdminRole.Id);
            
            if (!hasSuperAdminRole)
            {
                var userRole = UserRole.Create(existingUser.Id, superAdminRole.Id);
                await context.UserRoles.AddAsync(userRole);
                await context.SaveChangesAsync();
                
                Console.WriteLine($"✅ {adminRoleName} role assigned to existing admin user!");
            }
            else
            {
                Console.WriteLine("✅ Admin user already has SuperAdmin role");
            }
        }
    }
}
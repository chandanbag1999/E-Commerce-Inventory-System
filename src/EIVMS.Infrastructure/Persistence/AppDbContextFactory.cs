using EIVMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EIVMS.Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql("Host=ep-damp-tooth-a8xkl1iy-pooler.eastus2.azure.neon.tech;Database=neondb;Username=neondb_owner;Password=npg_hX3Qavp7izCS;SSLMode=Require");
        return new AppDbContext(optionsBuilder.Options);
    }
}
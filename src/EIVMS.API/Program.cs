using EIVMS.Infrastructure;
using EIVMS.Infrastructure.Persistence;
using EIVMS.Infrastructure.Seeders;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

try
{
    builder.Services.AddInfrastructure(builder.Configuration);
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to add infrastructure: {ex.Message}");
}

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(EIVMS.Application.Modules.Orders.Services.OrderService).Assembly));

builder.Services.AddOpenApi();

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Development", policy =>
    {
        policy.WithOrigins("http://localhost:8080", "http://localhost:8081", "http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (app.Environment.IsDevelopment())
    {
        try
        {
            Console.WriteLine("Creating database tables...");
            await db.Database.EnsureCreatedAsync();
            Console.WriteLine("Database tables created.");
            
            var connection = db.Database.GetDbConnection();
            connection.Open();
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"ALTER TABLE ""Users"" ADD COLUMN IF NOT EXISTS ""Status"" integer NOT NULL DEFAULT 1";
                cmd.ExecuteNonQuery();
                Console.WriteLine("Added Status column to Users table.");
                
                cmd.CommandText = @"CREATE TABLE IF NOT EXISTS ""Categories"" (
                    ""Id"" uuid NOT NULL,
                    ""Name"" varchar(200) NOT NULL,
                    ""Slug"" varchar(200) NOT NULL,
                    ""Description"" varchar(2000),
                    ""ParentId"" uuid,
                    ""DisplayOrder"" integer NOT NULL DEFAULT 0,
                    ""ImageUrl"" varchar(500),
                    ""MetaTitle"" varchar(200),
                    ""MetaDescription"" varchar(500),
                    ""MetaKeywords"" varchar(500),
                    ""IsActive"" boolean NOT NULL DEFAULT true,
                    ""IsDeleted"" boolean NOT NULL DEFAULT false,
                    ""CommissionRate"" numeric(5,2),
                    ""CreatedAt"" timestamp with time zone NOT NULL,
                    ""UpdatedAt"" timestamp with time zone,
                    PRIMARY KEY (""Id""),
                    CONSTRAINT ""FK_Categories_Categories_ParentId"" FOREIGN KEY (""ParentId"") 
                        REFERENCES ""Categories"" (""Id"") ON DELETE RESTRICT
                )";
                cmd.ExecuteNonQuery();
                Console.WriteLine("Created Categories table.");
                
                cmd.CommandText = @"CREATE INDEX IF NOT EXISTS ""IX_Categories_ParentId"" ON ""Categories"" (""ParentId"")";
                cmd.ExecuteNonQuery();
                cmd.CommandText = @"CREATE INDEX IF NOT EXISTS ""IX_Categories_IsActive"" ON ""Categories"" (""IsActive"")";
                cmd.ExecuteNonQuery();
                cmd.CommandText = @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Categories_Slug"" ON ""Categories"" (""Slug"")";
                cmd.ExecuteNonQuery();
                Console.WriteLine("Created Categories indexes.");
            }
            connection.Close();
            
            await RolePermissionSeeder.SeedAsync(db);
            await UserSeeder.SeedAsync(db, builder.Configuration);
            await ProductCatalogSeeder.SeedAsync(db);
            await WarehouseSeeder.SeedAsync(db);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database seeding error: {ex.Message}");
            Console.WriteLine($"Stack: {ex.StackTrace}");
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "EIVMS API";
        options.Theme = ScalarTheme.Purple;
    });
}

app.UseHttpsRedirection();

app.UseCors("Development");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
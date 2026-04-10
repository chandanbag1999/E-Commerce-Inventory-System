using EIVMS.Infrastructure;
using EIVMS.Infrastructure.Persistence;
using EIVMS.Infrastructure.Seeders;
using Microsoft.EntityFrameworkCore;
using MediatR;

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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Development", policy =>
    {
        policy.WithOrigins("http://localhost:8080")
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
            }
            connection.Close();
            
            await RolePermissionSeeder.SeedAsync(db);
            await UserSeeder.SeedAsync(db, builder.Configuration);
            await ProductCatalogSeeder.SeedAsync(db);
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
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("Development");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
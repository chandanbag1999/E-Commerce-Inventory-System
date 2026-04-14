using EcommerceInventory.Application.DependencyInjections;
using EcommerceInventory.Infrastructure;
using EcommerceInventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/ecommerce-inventory-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add Application services (MediatR, AutoMapper, Validators)
builder.Services.AddApplicationServices();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Register Infrastructure services
builder.Services.AddInfrastructureServices(builder.Configuration);

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Auto-apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    Log.Information("Applying database migrations...");
    await db.Database.MigrateAsync();
    Log.Information("Database migrations applied successfully.");
}

// Configure the HTTP request pipeline
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication(); // Add authentication middleware
app.UseAuthorization();

// Map OpenAPI endpoint
app.MapOpenApi();

// Add Scalar API documentation for all environments
app.MapScalarApiReference(options =>
{
    options.WithTitle("Ecommerce Inventory API");
    options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});

app.MapControllers();

app.Run();

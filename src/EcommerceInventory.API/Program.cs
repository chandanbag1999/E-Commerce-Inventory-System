using EcommerceInventory.API.Extensions;
using EcommerceInventory.API.Middleware;
using EcommerceInventory.Application.DependencyInjections;
using EcommerceInventory.Infrastructure;
using EcommerceInventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────
// Serilog Configuration
// ─────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/ecommerce-inventory-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog((context, loggerConfig) =>
{
    loggerConfig
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console()
        .WriteTo.File("logs/ecommerce-inventory-.txt", rollingInterval: RollingInterval.Day);
});

// ─────────────────────────────────────────────
// Application Layer Services (MediatR, AutoMapper, Validators)
// ─────────────────────────────────────────────
builder.Services.AddApplicationServices();

// ─────────────────────────────────────────────
// Infrastructure Layer Services (DbContext, Repositories, JWT Auth, Services)
// ─────────────────────────────────────────────
builder.Services.AddInfrastructureServices(builder.Configuration);

// ─────────────────────────────────────────────
// API Layer Services
// ─────────────────────────────────────────────

// Permission Authorization (handler + policies for all 41 permissions)
builder.Services.AddPermissionAuthorization();

// Register all permission policies dynamically
var allPermissions = new[]
{
    "Users.View", "Users.Create", "Users.Edit", "Users.Delete", "Users.AssignRole",
    "Roles.View",
    "Categories.View", "Categories.Create", "Categories.Edit", "Categories.Delete",
    "Products.View", "Products.Create", "Products.Edit", "Products.Delete",
    "Warehouses.View", "Warehouses.Create", "Warehouses.Edit", "Warehouses.Delete",
    "Stocks.View", "Stocks.Adjust",
    "Suppliers.View", "Suppliers.Create", "Suppliers.Edit", "Suppliers.Delete",
    "PurchaseOrders.View", "PurchaseOrders.Create", "PurchaseOrders.Approve",
    "PurchaseOrders.Receive", "PurchaseOrders.Cancel",
    "SalesOrders.View", "SalesOrders.Create", "SalesOrders.Approve",
    "SalesOrders.Ship", "SalesOrders.Deliver", "SalesOrders.Cancel"
};

builder.Services.AddPermissionPolicies(allPermissions);

// Controllers + OpenAPI
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// CORS (already registered in Infrastructure, but keeping explicit here)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ─────────────────────────────────────────────
// Build the application
// ─────────────────────────────────────────────
var app = builder.Build();

// ─────────────────────────────────────────────
// Auto-apply migrations on startup
// ─────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    Log.Information("Applying database migrations...");
    await db.Database.MigrateAsync();
    Log.Information("Database migrations applied successfully.");
}

// ─────────────────────────────────────────────
// Seed SuperAdmin (if enabled in configuration)
// ─────────────────────────────────────────────
var enableSeeder = app.Configuration.GetValue<bool>("AppSettings:EnableSuperAdminSeeder");
if (enableSeeder)
{
    Log.Information("Running SuperAdmin seeder...");
    await EcommerceInventory.Infrastructure.Persistence.Seed.SuperAdminSeeder.SeedSuperAdminAsync(app.Services);
    Log.Information("SuperAdmin seeder completed.");
}

// ─────────────────────────────────────────────
// Middleware Pipeline (order matters!)
// ─────────────────────────────────────────────

// 1. Global Exception Handler — MUST be first to catch all errors
app.UseGlobalExceptionHandler();

// 2. Correlation ID — adds tracing ID to every request
app.UseCorrelationId();

// 3. HTTPS Redirection
app.UseHttpsRedirection();

// 4. CORS
app.UseCors();

// 5. Authentication — validates JWT tokens
app.UseAuthentication();

// 6. Authorization — checks policies/permissions
app.UseAuthorization();

// ─────────────────────────────────────────────
// Endpoints
// ─────────────────────────────────────────────

// OpenAPI schema endpoint
app.MapOpenApi();

// Scalar API documentation (with JWT Bearer auth support)
app.MapScalarApiReference(options =>
{
    options.WithTitle("Ecommerce Inventory API");
    options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    // Scalar automatically detects JWT auth from the OpenAPI schema
});

// Map all API controllers
app.MapControllers();

Log.Information("Starting Ecommerce Inventory API...");
app.Run();

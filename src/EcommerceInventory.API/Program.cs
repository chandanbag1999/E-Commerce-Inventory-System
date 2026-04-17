using EcommerceInventory.API.Middleware;
using EcommerceInventory.Application;
using EcommerceInventory.Infrastructure;
using EcommerceInventory.Infrastructure.Persistence;
using EcommerceInventory.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .WriteTo.Console()
        .WriteTo.File("logs/log-.txt",
                      rollingInterval: RollingInterval.Day,
                      retainedFileCountLimit: 7)
        .Enrich.FromLogContext()
    );

    builder.Services.AddApplication(builder.Configuration);
    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new()
        {
            Title       = "Ecommerce Inventory API",
            Version     = "v1",
            Description = "Enterprise Ecommerce Inventory Management System API"
        });

        c.AddSecurityDefinition("Bearer", new()
        {
            Name         = "Authorization",
            Type         = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme       = "Bearer",
            BearerFormat = "JWT",
            In           = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description  = "Enter your JWT token"
        });

        c.AddSecurityRequirement(new()
        {
            {
                new()
                {
                    Reference = new()
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id   = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader());
    });

    var app = builder.Build();

    // ── Database Migration + SuperAdmin Seed
    using (var scope = app.Services.CreateScope())
    {
        var db            = scope.ServiceProvider
                                 .GetRequiredService<AppDbContext>();
        var configuration = scope.ServiceProvider
                                 .GetRequiredService<IConfiguration>();
        var logger        = scope.ServiceProvider
                                 .GetRequiredService<ILogger<Program>>();

        // 1. Apply all pending EF migrations
        await db.Database.MigrateAsync();
        Log.Information("Database migration applied successfully.");

        // 2. Seed SuperAdmin user (only if not exists)
        await SuperAdminSeeder.SeedAsync(db, configuration, logger);
        Log.Information("SuperAdmin seeding completed.");
    }

    app.UseSerilogRequestLogging();
    app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json",
                          "Ecommerce Inventory API v1");
        c.RoutePrefix = string.Empty;
    });

    app.UseCors("AllowAll");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    Log.Information("Application starting...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application startup failed.");
}
finally
{
    Log.CloseAndFlush();
}

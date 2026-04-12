using System.IO;
using EIVMS.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using EIVMS.Application.Modules.Identity.Interfaces;
using EIVMS.Application.Modules.Identity.Services;
using EIVMS.Application.Modules.Identity.Validators;
using EIVMS.Application.Modules.UserManagement.Interfaces;
using EIVMS.Application.Modules.UserManagement.Services;
using EIVMS.Application.Modules.Analytics.Interfaces;
using EIVMS.Application.Modules.ProductCatalog.Interfaces;
using EIVMS.Application.Modules.ProductCatalog.Services;
using EIVMS.Application.Modules.Inventory.Interfaces;
using EIVMS.Application.Modules.Inventory.Services;
using EIVMS.Application.Modules.Notifications.Interfaces;
using EIVMS.Application.Modules.Notifications.Services;
using EIVMS.Application.Modules.Orders.Interfaces;
using EIVMS.Application.Modules.Orders.Services;
using EIVMS.Application.Modules.Orders.Validators;
using EIVMS.Application.Modules.Payments.Interfaces;
using EIVMS.Application.Modules.Payments.Services;
using EIVMS.Application.Modules.Payments.Validators;
using EIVMS.Infrastructure.Persistence;
using EIVMS.Infrastructure.Repositories;
using EIVMS.Infrastructure.Repositories.UserManagement;
using EIVMS.Infrastructure.Repositories.ProductCatalog;
using EIVMS.Infrastructure.Repositories.Inventory;
using EIVMS.Infrastructure.Repositories.Notifications;
using EIVMS.Infrastructure.Repositories.Orders;
using EIVMS.Infrastructure.Repositories.Payments;
using EIVMS.Infrastructure.Services;
using EIVMS.Infrastructure.Services.Analytics;
using EIVMS.Infrastructure.Services.UserManagement;
using EIVMS.Infrastructure.Services.Payments;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EIVMS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorCodesToAdd: null);
                }));

        services.AddHttpContextAccessor();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<IUserManagementRepository, UserManagementRepository>();
        services.AddScoped<IUserProfileService, UserProfileService>();
        services.AddScoped<IAddressService, AddressService>();
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<IVendorService, VendorService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();

        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductService, ProductService>();

        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<IInventoryService, InventoryService>();

        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationService, NotificationService>();

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IInventoryIntegrationService, EIVMS.Application.Modules.Orders.Services.Integration.InventoryIntegrationService>();
        services.AddScoped<IProductIntegrationService, EIVMS.Application.Modules.Orders.Services.Integration.ProductIntegrationService>();

        services.Configure<PaymentSettings>(configuration.GetSection("Payment"));

        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IWebhookService, WebhookService>();
        services.AddScoped<IRefundService, RefundService>();
        services.AddScoped<IReconciliationService, ReconciliationService>();
        
        services.AddSingleton<IPaymentGatewayFactory, PaymentGatewayFactory>();
        services.AddHttpClient<RazorpayGatewayService>();
        services.AddHttpClient<StripeGatewayService>();
        services.AddSingleton<IPaymentGateway, RazorpayGatewayService>();
        services.AddSingleton<IPaymentGateway, StripeGatewayService>();
        
        services.AddSingleton<IWebhookIdempotencyStore, InMemoryWebhookIdempotencyStore>();
        services.AddHostedService<ReconciliationBackgroundService>();

        services.AddSingleton<IJwtService, JwtService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<EIVMS.Application.Modules.ProductCatalog.Validators.CreateProductValidator>();
        services.AddValidatorsFromAssemblyContaining<EIVMS.Application.Modules.Orders.Validators.CreateOrderValidator>();
        services.AddValidatorsFromAssemblyContaining<CreatePaymentValidator>();
        services.AddValidatorsFromAssemblyContaining<RefundRequestValidator>();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]!)),
                ClockSkew = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                OnChallenge = context =>
                {
                    context.HandleResponse();
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    var result = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        success = false,
                        message = "Unauthorized - Valid JWT token required",
                        statusCode = 401
                    });
                    return context.Response.WriteAsync(result);
                },
                OnForbidden = context =>
                {
                    context.Response.StatusCode = 403;
                    context.Response.ContentType = "application/json";
                    var result = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        success = false,
                        message = "Forbidden - Insufficient permissions",
                        statusCode = 403
                    });
                    return context.Response.WriteAsync(result);
                }
            };
        });

        services.AddAuthorization(options =>
        {
            var permissions = new[]
            {
                "product:create", "product:read", "product:update", "product:delete",
                "inventory:warehouse:create", "inventory:warehouse:update", "inventory:warehouse:delete",
                "inventory:item:create", "inventory:item:update",
                "inventory:stock:create", "inventory:reservation:create", "inventory:reservation:update",
                "inventory:transfer:create", "inventory:transfer:update",
                "inventory:alert:update", "inventory:admin",
                "order:create", "order:read", "order:update", "order:cancel",
                "user:manage", "user:view",
                "report:generate", "report:view"
            };

            foreach (var permission in permissions)
            {
                options.AddPolicy(permission, policy =>
                    policy.RequireClaim("permission", permission));
            }

            options.AddPolicy("orders:read", policy =>
                policy.RequireClaim("permission", "order:read"));
            options.AddPolicy("orders:update", policy =>
                policy.RequireClaim("permission", "order:update"));

            options.AddPolicy("AdminOnly", policy =>
                policy.RequireRole("Admin"));

            options.AddPolicy("VendorOrAdmin", policy =>
                policy.RequireRole("Admin", "Vendor"));
        });

        return services;
    }
}

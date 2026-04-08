using System.IO;
using EIVMS.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using EIVMS.Application.Modules.Identity.Interfaces;
using EIVMS.Application.Modules.Identity.Services;
using EIVMS.Application.Modules.Identity.Validators;
using EIVMS.Infrastructure.Persistence;
using EIVMS.Infrastructure.Repositories;
using EIVMS.Infrastructure.Services;
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

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddSingleton<IJwtService, JwtService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

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
                "inventory:manage", "inventory:view",
                "order:create", "order:read", "order:update", "order:cancel",
                "user:manage", "user:view",
                "report:generate", "report:view"
            };

            foreach (var permission in permissions)
            {
                options.AddPolicy(permission, policy =>
                    policy.RequireClaim("permission", permission));
            }

            options.AddPolicy("AdminOnly", policy =>
                policy.RequireRole("Admin"));

            options.AddPolicy("VendorOrAdmin", policy =>
                policy.RequireRole("Admin", "Vendor"));
        });

        return services;
    }
}
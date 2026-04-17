using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Infrastructure.Identity;
using EcommerceInventory.Infrastructure.Persistence;
using EcommerceInventory.Infrastructure.Persistence.Repositories;
using EcommerceInventory.Infrastructure.Services;
using EcommerceInventory.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EcommerceInventory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtSettings>(
            configuration.GetSection("JwtSettings"));
        services.Configure<EmailSettings>(
            configuration.GetSection("EmailSettings"));
        services.Configure<CloudinarySettings>(
            configuration.GetSection("CloudinarySettings"));
        services.Configure<AppSettings>(
            configuration.GetSection("AppSettings"));

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsAssembly(
                    typeof(AppDbContext).Assembly.FullName)
            )
        );

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ICloudinaryService, CloudinaryService>();
        services.AddScoped<IDateTimeService, DateTimeService>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        var jwtSettings = configuration
                              .GetSection("JwtSettings")
                              .Get<JwtSettings>()!;

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer           = true,
                ValidateAudience         = true,
                ValidateLifetime         = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer              = jwtSettings.Issuer,
                ValidAudience            = jwtSettings.Audience,
                IssuerSigningKey         = new SymmetricSecurityKey(
                                               Encoding.UTF8.GetBytes(
                                                   jwtSettings.SecretKey)),
                ClockSkew                = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception is SecurityTokenExpiredException)
                        context.Response.Headers["Token-Expired"] = "true";
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorizationBuilder();

        return services;
    }
}

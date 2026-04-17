using EcommerceInventory.Application.Common.Behaviours;
using EcommerceInventory.Application.Common.Settings;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EcommerceInventory.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtSettings>(
            configuration.GetSection("JwtSettings"));

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(
                Assembly.GetExecutingAssembly()));

        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(LoggingBehaviour<,>));

        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehaviour<,>));

        services.AddValidatorsFromAssembly(
            Assembly.GetExecutingAssembly());

        return services;
    }
}

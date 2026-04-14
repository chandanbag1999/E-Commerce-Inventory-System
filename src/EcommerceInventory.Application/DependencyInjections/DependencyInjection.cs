using System.Reflection;
using EcommerceInventory.Application.Common.Behaviors;
using EcommerceInventory.Application.Common.Mapping;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EcommerceInventory.Application.DependencyInjections;

/// <summary>
/// Extension method to register all Application layer services
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        // Register pipeline behaviors
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

        // Register FluentValidation validators
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Register AutoMapper
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });

        return services;
    }
}

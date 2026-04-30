using Delivery.API.Application.Interfaces;
using Delivery.API.Application.Services;
using Delivery.API.Infrastructure.Consumers;
using Delivery.API.Infrastructure.Persistence;
using Delivery.API.Infrastructure.Persistence.Repositories;
using FoodFleet.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.API;

/// <summary>
/// Extension methods for registering Delivery API services with the dependency injection container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all Delivery API services including the database context, repositories,
    /// application services, and RabbitMQ message consumers.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The application configuration for connection strings and settings.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddDeliveryServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<DeliveryDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IDeliveryService, DeliveryService>();

        services.AddRabbitMqMessaging(configuration, x =>
        {
            x.AddConsumer<OrderReadyConsumer>();
        });

        return services;
    }
}

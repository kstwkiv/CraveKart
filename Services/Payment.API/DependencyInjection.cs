using FoodFleet.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payment.API.Application.Interfaces;
using Payment.API.Application.Services;
using Payment.API.Infrastructure.Consumers;
using Payment.API.Infrastructure.Persistence;
using Payment.API.Infrastructure.Persistence.Repositories;

namespace Payment.API;

/// <summary>
/// Extension methods for registering Payment API services with the dependency injection container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all Payment API services including the database context, repositories,
    /// application services, and RabbitMQ message consumers.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The application configuration for connection strings and settings.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddPaymentServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<PaymentDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPaymentService, PaymentService>();

        services.AddRabbitMqMessaging(configuration, x =>
        {
            x.AddConsumer<OrderPlacedConsumer>();
            x.AddConsumer<OrderCancelledConsumer>();
            x.AddConsumer<DeliveryCompletedConsumer>();
        });

        return services;
    }
}

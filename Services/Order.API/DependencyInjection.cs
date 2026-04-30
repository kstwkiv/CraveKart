using FluentValidation;
using FoodFleet.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Order.API.Application.Interfaces;
using Order.API.Application.Services;
using Order.API.Infrastructure.Consumers;
using Order.API.Infrastructure.Persistence;
using Order.API.Infrastructure.Persistence.Repositories;

namespace Order.API;

/// <summary>
/// Extension methods for registering Order API services with the dependency injection container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all Order API services including the database context, repositories,
    /// application services, validators, and RabbitMQ message consumers.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The application configuration for connection strings and settings.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddOrderServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<OrderDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IOrderService, OrderService>();

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddRabbitMqMessaging(configuration, x =>
        {
            x.AddConsumer<PaymentConfirmedConsumer>();
            x.AddConsumer<PaymentFailedConsumer>();
            x.AddConsumer<DeliveryCompletedConsumer>();
        });

        return services;
    }
}

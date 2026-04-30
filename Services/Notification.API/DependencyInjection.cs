using FoodFleet.Shared.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notification.API.Application.Interfaces;
using Notification.API.Infrastructure.Consumers;
using Notification.API.Infrastructure.Services;

namespace Notification.API;

/// <summary>
/// Extension methods for registering Notification API services with the dependency injection container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all Notification API services including the email service and all RabbitMQ event consumers.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The application configuration for connection strings and settings.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddNotificationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IEmailService, EmailService>();

        services.AddRabbitMqMessaging(configuration, x =>
        {
            x.AddConsumer<UserRegisteredConsumer>();
            x.AddConsumer<OrderPlacedConsumer>();
            x.AddConsumer<OrderStatusChangedConsumer>();
            x.AddConsumer<OrderCancelledConsumer>();
            x.AddConsumer<PaymentFailedConsumer>();
            x.AddConsumer<PaymentRefundedConsumer>();
            x.AddConsumer<DeliveryCompletedConsumer>();
            x.AddConsumer<RestaurantApprovedConsumer>();
            x.AddConsumer<RestaurantRejectedConsumer>();
        });

        return services;
    }
}
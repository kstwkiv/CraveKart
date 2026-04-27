using FoodFleet.Shared.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notification.API.Application.Interfaces;
using Notification.API.Infrastructure.Consumers;
using Notification.API.Infrastructure.Services;

namespace Notification.API;

public static class DependencyInjection
{
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
            x.AddConsumer<DeliveryCompletedConsumer>();
            x.AddConsumer<RestaurantApprovedConsumer>();
            x.AddConsumer<RestaurantRejectedConsumer>();
        });

        return services;
    }
}
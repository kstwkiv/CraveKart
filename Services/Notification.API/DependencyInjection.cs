// 'using' — imports the shared messaging namespace that contains AddRabbitMqMessaging extension method
using FoodFleet.Shared.Messaging;
// 'using' — imports Microsoft's configuration abstractions (IConfiguration) for reading appsettings
using Microsoft.Extensions.Configuration;
// 'using' — imports the DI container abstractions (IServiceCollection, AddScoped, etc.)
using Microsoft.Extensions.DependencyInjection;
// 'using' — imports the IEmailService interface so it can be registered in the DI container
using Notification.API.Application.Interfaces;
// 'using' — imports all message consumer classes that listen to RabbitMQ queues
using Notification.API.Infrastructure.Consumers;
// 'using' — imports the concrete EmailService implementation
using Notification.API.Infrastructure.Services;

// 'namespace' — scopes this class to the Notification.API assembly root namespace
namespace Notification.API;

/// <summary>
/// Extension methods for registering Notification API services with the dependency injection container.
/// </summary>
// 'public' — visible to the host project (Program.cs) that calls the extension method
// 'static' — a static class cannot be instantiated; it exists purely as a container for static members
//             Extension methods must live in a static class
public static class DependencyInjection
{
    /// <summary>
    /// Registers all Notification API services including the email service and all RabbitMQ event consumers.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The application configuration for connection strings and settings.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    // 'public' — the extension method must be public so the host project can call it
    // 'static' — extension methods must be static; the 'this' parameter makes it callable on IServiceCollection
    // 'IServiceCollection' — the DI container's registration surface; methods on it register service lifetimes
    // 'this' — the 'this' modifier on the first parameter is what makes this an extension method
    public static IServiceCollection AddNotificationServices(
        this IServiceCollection services,
        // 'IConfiguration' — abstraction over appsettings.json / environment variables / secrets
        IConfiguration configuration)
    {
        // 'AddScoped' — registers IEmailService with a Scoped lifetime:
        //               one instance per HTTP request (or per DI scope), shared within that scope
        services.AddScoped<IEmailService, EmailService>();

        // 'AddRabbitMqMessaging' — extension method that wires up the RabbitMQ message bus
        // The lambda 'x => { ... }' configures which consumers are registered with the bus
        services.AddRabbitMqMessaging(configuration, x =>
        {
            // 'AddConsumer' — registers each consumer class so the bus knows which queues to subscribe to
            x.AddConsumer<UserRegisteredConsumer>();
            x.AddConsumer<OrderPlacedConsumer>();
            x.AddConsumer<OrderStatusChangedConsumer>();
            x.AddConsumer<OrderCancelledConsumer>();
            x.AddConsumer<PaymentConfirmedConsumer>();
            x.AddConsumer<PaymentFailedConsumer>();
            x.AddConsumer<PaymentRefundedConsumer>();
            x.AddConsumer<DeliveryCompletedConsumer>();
            x.AddConsumer<RestaurantApprovedConsumer>();
            x.AddConsumer<RestaurantRejectedConsumer>();
        });

        // 'return' — returns the same IServiceCollection so calls can be chained fluently in Program.cs
        return services;
    }
}

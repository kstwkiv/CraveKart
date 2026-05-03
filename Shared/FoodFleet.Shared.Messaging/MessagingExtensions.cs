using FoodFleet.Shared.Messaging.Configuration;
using FoodFleet.Shared.Messaging.Interfaces;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FoodFleet.Shared.Messaging;

/// <summary>
/// Extension methods for registering RabbitMQ messaging infrastructure with the dependency injection container.
/// Configures MassTransit with RabbitMQ transport and registers <see cref="IEventPublisher"/>.
/// </summary>
public static class MessagingExtensions
{
    /// <summary>
    /// Registers MassTransit with RabbitMQ transport, optional consumers, and the <see cref="IEventPublisher"/> service.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The application configuration containing the "RabbitMq" section.</param>
    /// <param name="configureConsumers">Optional action to register MassTransit consumers.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    /// <exception cref="Exception">Thrown when the "RabbitMq" configuration section is missing.</exception>
    public static IServiceCollection AddRabbitMqMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator>? configureConsumers = null)
    {
        var settings = configuration
        .GetSection("RabbitMq")
        .Get<RabbitMqSettings>();

        if (settings == null)
        {
            throw new Exception("RabbitMq configuration is missing!");
        }

        services.AddMassTransit(x =>
        {
            configureConsumers?.Invoke(x);

            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(settings.Host, settings.Port, "/", h =>
                {
                    h.Username(settings.Username);
                    h.Password(settings.Password);
                });

                cfg.ConfigureEndpoints(ctx);
            });
        });

        services.AddScoped<IEventPublisher, EventPublisher>();

        return services;
    }
}
using FoodFleet.Shared.Messaging.Configuration;
using FoodFleet.Shared.Messaging.Interfaces;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FoodFleet.Shared.Messaging;

public static class MessagingExtensions
{
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
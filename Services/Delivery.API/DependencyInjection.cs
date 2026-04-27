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

public static class DependencyInjection
{
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

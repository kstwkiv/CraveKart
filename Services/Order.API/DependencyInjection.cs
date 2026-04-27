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

public static class DependencyInjection
{
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

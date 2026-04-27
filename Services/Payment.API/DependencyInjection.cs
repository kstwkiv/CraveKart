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

public static class DependencyInjection
{
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
        });

        return services;
    }
}

using FluentValidation;
using FoodFleet.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Restaurant.API.Application.Interfaces;
using Restaurant.API.Application.Services;
using Restaurant.API.Infrastructure.Consumers;
using Restaurant.API.Infrastructure.Persistence;
using Restaurant.API.Infrastructure.Persistence.Repositories;

namespace Restaurant.API;

public static class DependencyInjection
{
    public static IServiceCollection AddRestaurantServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<RestaurantDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IRestaurantService, RestaurantService>();

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddRabbitMqMessaging(configuration, x =>
        {
            x.AddConsumer<UserRegisteredConsumer>();
        });

        return services;
    }
}

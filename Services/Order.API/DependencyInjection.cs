// 'using' — imports FluentValidation so AddValidatorsFromAssembly is available for registering validators
using FluentValidation;
// 'using' — imports the shared messaging namespace that provides AddRabbitMqMessaging extension method
using FoodFleet.Shared.Messaging;
// 'using' — imports Entity Framework Core so AddDbContext and UseSqlServer are available
using Microsoft.EntityFrameworkCore;
// 'using' — imports IConfiguration abstraction for reading connection strings from appsettings
using Microsoft.Extensions.Configuration;
// 'using' — imports the DI container abstractions (IServiceCollection, AddScoped, etc.)
using Microsoft.Extensions.DependencyInjection;
// 'using' — imports the IUnitOfWork interface for registering the Unit of Work pattern
using Order.API.Application.Interfaces;
// 'using' — imports the IOrderService interface for registering the order application service
using Order.API.Application.Services;
// 'using' — imports RabbitMQ consumer classes that react to payment and delivery events
using Order.API.Infrastructure.Consumers;
// 'using' — imports the EF Core DbContext for the Order service
using Order.API.Infrastructure.Persistence;
// 'using' — imports the concrete repository implementations used by the Unit of Work
using Order.API.Infrastructure.Persistence.Repositories;

// 'namespace' — scopes this class to the Order.API assembly root namespace
namespace Order.API;

/// <summary>
/// Extension methods for registering Order API services with the dependency injection container.
/// </summary>
// 'public' — visible to the host project (Program.cs) that calls the extension method
// 'static' — a static class cannot be instantiated; it is a pure container for static utility methods
//             Extension methods are required to live in a static class
public static class DependencyInjection
{
    /// <summary>
    /// Registers all Order API services including the database context, repositories,
    /// application services, validators, and RabbitMQ message consumers.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The application configuration for connection strings and settings.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    // 'public' — the extension method must be public so Program.cs can call it on IServiceCollection
    // 'static' — extension methods must be static; the 'this' keyword on the first parameter enables the extension syntax
    // 'IServiceCollection' — the DI container's registration surface provided by Microsoft.Extensions.DependencyInjection
    public static IServiceCollection AddOrderServices(
        this IServiceCollection services,
        // 'IConfiguration' — abstraction over appsettings.json, environment variables, and secrets
        IConfiguration configuration)
    {
        // 'AddDbContext' — registers OrderDbContext with the DI container using a Scoped lifetime (one per request)
        // The lambda configures EF Core to use SQL Server with the connection string from configuration
        services.AddDbContext<OrderDbContext>(options =>
            // 'UseSqlServer' — configures EF Core's database provider; reads the connection string by key
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // 'AddScoped' — registers IUnitOfWork → UnitOfWork with Scoped lifetime:
        //               one instance per HTTP request, ensuring all repositories share the same DbContext
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        // 'AddScoped' — registers IOrderService → OrderService; scoped so it shares the same UnitOfWork per request
        services.AddScoped<IOrderService, OrderService>();

        // 'AddValidatorsFromAssembly' — scans the assembly for all FluentValidation AbstractValidator<T> classes
        //                               and registers them automatically; 'typeof' gets the runtime Type of DependencyInjection
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // 'AddRabbitMqMessaging' — extension method that configures the RabbitMQ message bus
        // The lambda registers consumers that react to events published by other microservices
        services.AddRabbitMqMessaging(configuration, x =>
        {
            // 'AddConsumer' — registers each consumer so the bus subscribes to the corresponding queue/exchange
            x.AddConsumer<PaymentConfirmedConsumer>();
            x.AddConsumer<PaymentFailedConsumer>();
            x.AddConsumer<DeliveryCompletedConsumer>();
        });

        // 'return' — returns the same IServiceCollection to allow fluent method chaining in Program.cs
        return services;
    }
}

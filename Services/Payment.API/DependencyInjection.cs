// 'using' — imports the shared RabbitMQ/MassTransit messaging registration helpers
using FoodFleet.Shared.Messaging;
// 'using' — imports Entity Framework Core's DbContext extension methods (UseSqlServer, etc.)
using Microsoft.EntityFrameworkCore;
// 'using' — imports IConfiguration, the abstraction over appsettings.json / environment variables
using Microsoft.Extensions.Configuration;
// 'using' — imports IServiceCollection and extension methods for registering services with the DI container
using Microsoft.Extensions.DependencyInjection;
// 'using' — imports the IPaymentService interface (the abstraction registered against its implementation)
using Payment.API.Application.Interfaces;
// 'using' — imports the PaymentService concrete class that implements IPaymentService
using Payment.API.Application.Services;
// 'using' — imports the MassTransit consumer classes to be registered with the message bus
using Payment.API.Infrastructure.Consumers;
// 'using' — imports the EF Core DbContext for the Payment database
using Payment.API.Infrastructure.Persistence;
// 'using' — imports the repository implementations registered as IUnitOfWork
using Payment.API.Infrastructure.Persistence.Repositories;

// 'namespace' — scopes this class to the root Payment.API namespace
namespace Payment.API;

/// <summary>
/// Extension methods for registering Payment API services with the dependency injection container.
/// </summary>
// 'public' — visible to Program.cs and any other assembly that references this project
// 'static' — the class belongs to the type itself; no instance is ever created
//   Static classes are idiomatic for extension-method containers in .NET
public static class DependencyInjection
{
    /// <summary>
    /// Registers all Payment API services including the database context, repositories,
    /// application services, and RabbitMQ message consumers.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The application configuration for connection strings and settings.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    // 'public static' — extension method; 'this IServiceCollection services' makes it callable as services.AddPaymentServices(...)
    // 'IServiceCollection' — the DI container's registration interface; services are added here during startup
    // 'IConfiguration' — abstraction over configuration sources (appsettings.json, env vars, secrets)
    public static IServiceCollection AddPaymentServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // AddDbContext<T> — registers PaymentDbContext with the DI container using a scoped lifetime
        // options.UseSqlServer(...) — configures EF Core to use SQL Server with the given connection string
        // GetConnectionString("DefaultConnection") — reads the connection string from the "ConnectionStrings" section of config
        services.AddDbContext<PaymentDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // AddScoped<TInterface, TImplementation> — registers a service with a scoped lifetime:
        //   one instance per HTTP request (or per DI scope), shared within that scope
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPaymentService, PaymentService>();

        // AddRabbitMqMessaging — shared extension that configures MassTransit with RabbitMQ as the transport
        // The lambda registers the consumers that should listen on the bus
        services.AddRabbitMqMessaging(configuration, x =>
        {
            // AddConsumer<T> — tells MassTransit to create a receive endpoint for each consumer type
            x.AddConsumer<OrderPlacedConsumer>();
            x.AddConsumer<OrderCancelledConsumer>();
            x.AddConsumer<DeliveryCompletedConsumer>();
        });

        // 'return services' — returns the same IServiceCollection to allow method chaining (fluent API pattern)
        return services;
    }
}

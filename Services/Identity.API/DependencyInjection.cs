using FluentValidation;
using FoodFleet.Shared.Messaging;
using Identity.API.Application.Interfaces;
using Identity.API.Application.Services;
using Identity.API.Infrastructure.Persistence;
using Identity.API.Infrastructure.Persistence.Repositories;
using Identity.API.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.API;

/// <summary>
/// Extension methods for registering Identity API services with the dependency injection container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all Identity API services including the database context, repositories,
    /// application services, validators, and RabbitMQ messaging.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The application configuration for connection strings and settings.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddIdentityServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<IdentityDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null)));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddRabbitMqMessaging(configuration);

        return services;
    }
}

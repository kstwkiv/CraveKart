using FoodFleet.Shared.Events.Auth;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Restaurant.API.Infrastructure.Consumers;

public class UserRegisteredConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly ILogger<UserRegisteredConsumer> _logger;

    public UserRegisteredConsumer(ILogger<UserRegisteredConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        _logger.LogInformation("Restaurant service received UserRegistered event for {Email}",
            context.Message.Email);
        return Task.CompletedTask;
    }
}
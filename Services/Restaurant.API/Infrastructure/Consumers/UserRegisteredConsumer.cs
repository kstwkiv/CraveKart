using FoodFleet.Shared.Events.Auth;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Restaurant.API.Infrastructure.Consumers;

/// <summary>
/// MassTransit consumer that handles <see cref="UserRegisteredEvent"/> messages in the Restaurant service.
/// Currently logs the event for audit purposes; can be extended for future restaurant-specific logic.
/// </summary>
public class UserRegisteredConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly ILogger<UserRegisteredConsumer> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="UserRegisteredConsumer"/>.
    /// </summary>
    /// <param name="logger">The logger for diagnostic output.</param>
    public UserRegisteredConsumer(ILogger<UserRegisteredConsumer> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Processes the user registered event and logs the registration for audit purposes.
    /// </summary>
    /// <param name="context">The consume context containing the event message.</param>
    public Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        _logger.LogInformation("Restaurant service received UserRegistered event for {Email}",
            context.Message.Email);
        return Task.CompletedTask;
    }
}
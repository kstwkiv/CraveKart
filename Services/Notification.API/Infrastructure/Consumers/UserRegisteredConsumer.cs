using FoodFleet.Shared.Events.Auth;
using MassTransit;
using Notification.API.Application.Interfaces;
using Notification.API.Infrastructure.Services;

namespace Notification.API.Infrastructure.Consumers;

/// <summary>
/// MassTransit consumer that handles <see cref="UserRegisteredEvent"/> messages
/// and sends a welcome email to the newly registered user.
/// </summary>
public class UserRegisteredConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly IEmailService _emailService;

    /// <summary>
    /// Initializes a new instance of <see cref="UserRegisteredConsumer"/>.
    /// </summary>
    /// <param name="emailService">The email service for sending notifications.</param>
    public UserRegisteredConsumer(IEmailService emailService)
    {
        _emailService = emailService;
    }

    /// <summary>
    /// Processes the user registered event and sends a welcome email to the new user.
    /// </summary>
    /// <param name="context">The consume context containing the event message.</param>
    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        await _emailService.SendAsync(
            context.Message.Email,
            "Welcome to CraveKart! 🎉",
            EmailTemplates.Welcome(context.Message.FullName));
    }
}

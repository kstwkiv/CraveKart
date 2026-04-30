using FoodFleet.Shared.Events.Auth;
using MassTransit;
using Notification.API.Application.Interfaces;
using Notification.API.Infrastructure.Services;

namespace Notification.API.Infrastructure.Consumers;

/// <summary>
/// MassTransit consumer that handles <see cref="UserLoggedInEvent"/> messages
/// and sends a sign-in security alert email to the user.
/// </summary>
public class UserLoggedInConsumer : IConsumer<UserLoggedInEvent>
{
    private readonly IEmailService _emailService;

    /// <summary>
    /// Initializes a new instance of <see cref="UserLoggedInConsumer"/>.
    /// </summary>
    /// <param name="emailService">The email service for sending notifications.</param>
    public UserLoggedInConsumer(IEmailService emailService)
    {
        _emailService = emailService;
    }

    /// <summary>
    /// Processes the user logged in event and sends a sign-in alert email.
    /// </summary>
    /// <param name="context">The consume context containing the event message.</param>
    public async Task Consume(ConsumeContext<UserLoggedInEvent> context)
    {
        var msg = context.Message;
        await _emailService.SendAsync(
            msg.Email,
            "New Sign-In to Your CraveKart Account 🔐",
            EmailTemplates.LoginAlert(msg.FullName, msg.LoggedInAt.ToString("f")));
    }
}

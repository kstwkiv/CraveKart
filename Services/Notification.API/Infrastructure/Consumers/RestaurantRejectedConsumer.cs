using FoodFleet.Shared.Events.Restaurants;
using MassTransit;
using Notification.API.Application.Interfaces;
using Notification.API.Infrastructure.Services;

namespace Notification.API.Infrastructure.Consumers;

/// <summary>
/// MassTransit consumer that handles <see cref="RestaurantRejectedEvent"/> messages
/// and sends a rejection or suspension notification email to the restaurant owner.
/// </summary>
public class RestaurantRejectedConsumer : IConsumer<RestaurantRejectedEvent>
{
    private readonly IEmailService _emailService;

    /// <summary>
    /// Initializes a new instance of <see cref="RestaurantRejectedConsumer"/>.
    /// </summary>
    /// <param name="emailService">The email service for sending notifications.</param>
    public RestaurantRejectedConsumer(IEmailService emailService)
    {
        _emailService = emailService;
    }

    /// <summary>
    /// Processes the restaurant rejected event and sends an appropriate rejection or suspension email.
    /// </summary>
    /// <param name="context">The consume context containing the event message.</param>
    public async Task Consume(ConsumeContext<RestaurantRejectedEvent> context)
    {
        var msg = context.Message;
        var isSuspended = msg.Reason.StartsWith("Suspended:");
        var reason = msg.Reason.Replace("Suspended: ", "");

        var subject = isSuspended
            ? $"Your Restaurant Has Been Suspended — {msg.RestaurantName}"
            : $"Your Restaurant Application Was Not Approved — {msg.RestaurantName}";

        await _emailService.SendAsync(
            msg.OwnerEmail,
            subject,
            EmailTemplates.RestaurantRejected(msg.RestaurantName, reason, isSuspended));
    }
}

using FoodFleet.Shared.Events.Restaurants;
using MassTransit;
using Notification.API.Application.Interfaces;
using Notification.API.Infrastructure.Services;

namespace Notification.API.Infrastructure.Consumers;

/// <summary>
/// MassTransit consumer that handles <see cref="RestaurantApprovedEvent"/> messages
/// and sends an approval notification email to the restaurant owner.
/// </summary>
public class RestaurantApprovedConsumer : IConsumer<RestaurantApprovedEvent>
{
    private readonly IEmailService _emailService;

    /// <summary>
    /// Initializes a new instance of <see cref="RestaurantApprovedConsumer"/>.
    /// </summary>
    /// <param name="emailService">The email service for sending notifications.</param>
    public RestaurantApprovedConsumer(IEmailService emailService)
    {
        _emailService = emailService;
    }

    /// <summary>
    /// Processes the restaurant approved event and sends a congratulatory email to the owner.
    /// </summary>
    /// <param name="context">The consume context containing the event message.</param>
    public async Task Consume(ConsumeContext<RestaurantApprovedEvent> context)
    {
        var msg = context.Message;
        await _emailService.SendAsync(
            msg.OwnerEmail,
            $"🎉 Your Restaurant is Live on CraveKart!",
            EmailTemplates.RestaurantApproved(msg.RestaurantName, msg.ApprovedAt.ToString("f")));
    }
}

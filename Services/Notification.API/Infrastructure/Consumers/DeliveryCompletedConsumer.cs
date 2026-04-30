using FoodFleet.Shared.Events.Delivery;
using MassTransit;
using Notification.API.Application.Interfaces;
using Notification.API.Infrastructure.Services;

namespace Notification.API.Infrastructure.Consumers;

/// <summary>
/// MassTransit consumer that handles <see cref="DeliveryCompletedEvent"/> messages
/// and sends a delivery completion email to the customer.
/// </summary>
public class DeliveryCompletedConsumer : IConsumer<DeliveryCompletedEvent>
{
    private readonly IEmailService _emailService;

    /// <summary>
    /// Initializes a new instance of <see cref="DeliveryCompletedConsumer"/>.
    /// </summary>
    /// <param name="emailService">The email service for sending notifications.</param>
    public DeliveryCompletedConsumer(IEmailService emailService)
    {
        _emailService = emailService;
    }

    /// <summary>
    /// Processes the delivery completed event and sends a confirmation email to the customer.
    /// </summary>
    /// <param name="context">The consume context containing the event message.</param>
    public async Task Consume(ConsumeContext<DeliveryCompletedEvent> context)
    {
        var msg = context.Message;
        await _emailService.SendAsync(
            msg.CustomerEmail,
            "Your Order Has Been Delivered! 🎊",
            EmailTemplates.DeliveryCompleted(
                msg.OrderId.ToString(),
                msg.CompletedAt.ToString("f") + " IST"));
    }
}

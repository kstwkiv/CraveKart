using FoodFleet.Shared.Events.Orders;
using MassTransit;
using Notification.API.Application.Interfaces;
using Notification.API.Infrastructure.Services;

namespace Notification.API.Infrastructure.Consumers;

/// <summary>
/// MassTransit consumer that handles <see cref="OrderCancelledEvent"/> messages
/// and sends an order cancellation email to the customer.
/// </summary>
public class OrderCancelledConsumer : IConsumer<OrderCancelledEvent>
{
    private readonly IEmailService _emailService;

    /// <summary>
    /// Initializes a new instance of <see cref="OrderCancelledConsumer"/>.
    /// </summary>
    /// <param name="emailService">The email service for sending notifications.</param>
    public OrderCancelledConsumer(IEmailService emailService)
    {
        _emailService = emailService;
    }

    /// <summary>
    /// Processes the order cancelled event and sends a cancellation email to the customer.
    /// </summary>
    /// <param name="context">The consume context containing the event message.</param>
    public async Task Consume(ConsumeContext<OrderCancelledEvent> context)
    {
        var msg = context.Message;
        await _emailService.SendAsync(
            msg.CustomerEmail,
            $"Your Order Has Been Cancelled — CraveKart",
            EmailTemplates.OrderCancelled(
                msg.OrderId.ToString(),
                msg.CancelledAt.ToString("f"),
                msg.Reason));
    }
}

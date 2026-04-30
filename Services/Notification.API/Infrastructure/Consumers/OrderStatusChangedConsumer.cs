using FoodFleet.Shared.Events.Orders;
using MassTransit;
using Notification.API.Application.Interfaces;
using Notification.API.Infrastructure.Services;

namespace Notification.API.Infrastructure.Consumers;

/// <summary>
/// MassTransit consumer that handles <see cref="OrderStatusChangedEvent"/> messages
/// and sends an order status update email to the customer.
/// </summary>
public class OrderStatusChangedConsumer : IConsumer<OrderStatusChangedEvent>
{
    private readonly IEmailService _emailService;

    /// <summary>
    /// Initializes a new instance of <see cref="OrderStatusChangedConsumer"/>.
    /// </summary>
    /// <param name="emailService">The email service for sending notifications.</param>
    public OrderStatusChangedConsumer(IEmailService emailService)
    {
        _emailService = emailService;
    }

    /// <summary>
    /// Processes the order status changed event and sends a status update email to the customer.
    /// </summary>
    /// <param name="context">The consume context containing the event message.</param>
    public async Task Consume(ConsumeContext<OrderStatusChangedEvent> context)
    {
        var msg = context.Message;
        await _emailService.SendAsync(
            msg.CustomerEmail,
            $"Your Order is {msg.NewStatus} — CraveKart",
            EmailTemplates.OrderStatusChanged(msg.OrderId.ToString(), msg.NewStatus));
    }
}

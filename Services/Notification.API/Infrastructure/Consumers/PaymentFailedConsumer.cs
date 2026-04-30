using FoodFleet.Shared.Events.Payments;
using MassTransit;
using Notification.API.Application.Interfaces;
using Notification.API.Infrastructure.Services;

namespace Notification.API.Infrastructure.Consumers;

/// <summary>
/// MassTransit consumer that handles <see cref="PaymentFailedEvent"/> messages
/// and sends a payment failure notification email to the customer.
/// </summary>
public class PaymentFailedConsumer : IConsumer<PaymentFailedEvent>
{
    private readonly IEmailService _emailService;

    /// <summary>
    /// Initializes a new instance of <see cref="PaymentFailedConsumer"/>.
    /// </summary>
    /// <param name="emailService">The email service for sending notifications.</param>
    public PaymentFailedConsumer(IEmailService emailService)
    {
        _emailService = emailService;
    }

    /// <summary>
    /// Processes the payment failed event and sends a failure notification email to the customer.
    /// </summary>
    /// <param name="context">The consume context containing the event message.</param>
    public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
    {
        var msg = context.Message;
        await _emailService.SendAsync(
            msg.CustomerEmail,
            "Payment Failed — CraveKart ⚠️",
            EmailTemplates.PaymentFailed(msg.OrderId.ToString(), msg.Reason));
    }
}

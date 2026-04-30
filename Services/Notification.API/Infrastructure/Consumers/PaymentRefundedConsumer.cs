using FoodFleet.Shared.Events.Payments;
using MassTransit;
using Notification.API.Application.Interfaces;
using Notification.API.Infrastructure.Services;

namespace Notification.API.Infrastructure.Consumers;

/// <summary>
/// Handles <see cref="PaymentRefundedEvent"/> and sends a refund confirmation email.
/// </summary>
public class PaymentRefundedConsumer : IConsumer<PaymentRefundedEvent>
{
    private readonly IEmailService _emailService;

    public PaymentRefundedConsumer(IEmailService emailService) => _emailService = emailService;

    public async Task Consume(ConsumeContext<PaymentRefundedEvent> context)
    {
        var msg = context.Message;
        if (string.IsNullOrEmpty(msg.CustomerEmail)) return;
        await _emailService.SendAsync(
            msg.CustomerEmail,
            "Your Refund Has Been Processed 💸",
            EmailTemplates.PaymentRefunded(msg.OrderId.ToString(), msg.Amount, msg.RefundedAt.ToString("f")));
    }
}

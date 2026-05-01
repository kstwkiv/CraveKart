using FoodFleet.Shared.Events.Payments;
using MassTransit;
using Notification.API.Application.Interfaces;
using Notification.API.Infrastructure.Services;

namespace Notification.API.Infrastructure.Consumers;

/// <summary>
/// Handles <see cref="PaymentConfirmedEvent"/> and sends a payment confirmation email.
/// Fires for both UPI (immediate) and COD (on delivery) payments.
/// </summary>
public class PaymentConfirmedConsumer : IConsumer<PaymentConfirmedEvent>
{
    private readonly IEmailService _emailService;

    public PaymentConfirmedConsumer(IEmailService emailService) => _emailService = emailService;

    public async Task Consume(ConsumeContext<PaymentConfirmedEvent> context)
    {
        var msg = context.Message;

        if (string.IsNullOrEmpty(msg.CustomerEmail)) return;

        await _emailService.SendAsync(
            msg.CustomerEmail,
            "Payment Confirmed ✅ — CraveKart",
            EmailTemplates.PaymentConfirmed(
                msg.OrderId.ToString(),
                msg.Amount,
                msg.PaymentMethod,
                msg.ConfirmedAt.ToString("f")));
    }
}

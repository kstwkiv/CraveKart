using FoodFleet.Shared.Events.Payments;
using MassTransit;
using Notification.API.Application.Interfaces;
using Notification.API.Infrastructure.Services;

namespace Notification.API.Infrastructure.Consumers;

public class PaymentFailedConsumer : IConsumer<PaymentFailedEvent>
{
    private readonly IEmailService _emailService;

    public PaymentFailedConsumer(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
    {
        var msg = context.Message;
        await _emailService.SendAsync(
            msg.CustomerEmail,
            "Payment Failed — CraveKart ⚠️",
            EmailTemplates.PaymentFailed(msg.OrderId.ToString(), msg.Reason));
    }
}

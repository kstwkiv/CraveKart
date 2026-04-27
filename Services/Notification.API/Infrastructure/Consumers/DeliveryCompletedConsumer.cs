using FoodFleet.Shared.Events.Delivery;
using MassTransit;
using Notification.API.Application.Interfaces;
using Notification.API.Infrastructure.Services;

namespace Notification.API.Infrastructure.Consumers;

public class DeliveryCompletedConsumer : IConsumer<DeliveryCompletedEvent>
{
    private readonly IEmailService _emailService;

    public DeliveryCompletedConsumer(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task Consume(ConsumeContext<DeliveryCompletedEvent> context)
    {
        var msg = context.Message;
        await _emailService.SendAsync(
            msg.CustomerEmail,
            "Your Order Has Been Delivered! 🎊",
            EmailTemplates.DeliveryCompleted(
                msg.OrderId.ToString(),
                msg.CompletedAt.ToString("f")));
    }
}

using FoodFleet.Shared.Events.Orders;
using MassTransit;
using Notification.API.Application.Interfaces;
using Notification.API.Infrastructure.Services;

namespace Notification.API.Infrastructure.Consumers;

public class OrderCancelledConsumer : IConsumer<OrderCancelledEvent>
{
    private readonly IEmailService _emailService;

    public OrderCancelledConsumer(IEmailService emailService)
    {
        _emailService = emailService;
    }

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

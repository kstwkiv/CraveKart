using FoodFleet.Shared.Events.Payments;
using MassTransit;
using Microsoft.Extensions.Logging;
using Order.API.Application.Commands;
using Order.API.Application.Interfaces;
using Order.API.Domain.Enums;

namespace Order.API.Infrastructure.Consumers;

public class PaymentFailedConsumer : IConsumer<PaymentFailedEvent>
{
    private readonly IOrderService _orderService;
    private readonly ILogger<PaymentFailedConsumer> _logger;

    public PaymentFailedConsumer(IOrderService orderService, ILogger<PaymentFailedConsumer> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
    {
        _logger.LogInformation("Payment failed for Order {OrderId}", context.Message.OrderId);
        await _orderService.UpdateStatusAsync(
            new UpdateOrderStatusCommand(context.Message.OrderId, OrderStatus.Cancelled),
            context.CancellationToken);
    }
}

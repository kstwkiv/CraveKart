using FoodFleet.Shared.Events.Payments;
using MassTransit;
using Microsoft.Extensions.Logging;
using Order.API.Application.Commands;
using Order.API.Application.Interfaces;
using Order.API.Domain.Enums;

namespace Order.API.Infrastructure.Consumers;

public class PaymentConfirmedConsumer : IConsumer<PaymentConfirmedEvent>
{
    private readonly IOrderService _orderService;
    private readonly ILogger<PaymentConfirmedConsumer> _logger;

    public PaymentConfirmedConsumer(IOrderService orderService, ILogger<PaymentConfirmedConsumer> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentConfirmedEvent> context)
    {
        _logger.LogInformation("Payment confirmed for Order {OrderId}", context.Message.OrderId);
        await _orderService.UpdateStatusAsync(
            new UpdateOrderStatusCommand(context.Message.OrderId, OrderStatus.Confirmed),
            context.CancellationToken);
    }
}

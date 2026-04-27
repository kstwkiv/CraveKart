using FoodFleet.Shared.Events.Delivery;
using MassTransit;
using Microsoft.Extensions.Logging;
using Order.API.Application.Commands;
using Order.API.Application.Interfaces;
using Order.API.Domain.Enums;

namespace Order.API.Infrastructure.Consumers;

public class DeliveryCompletedConsumer : IConsumer<DeliveryCompletedEvent>
{
    private readonly IOrderService _orderService;
    private readonly ILogger<DeliveryCompletedConsumer> _logger;

    public DeliveryCompletedConsumer(IOrderService orderService, ILogger<DeliveryCompletedConsumer> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<DeliveryCompletedEvent> context)
    {
        _logger.LogInformation("Delivery completed for Order {OrderId}", context.Message.OrderId);
        await _orderService.UpdateStatusAsync(
            new UpdateOrderStatusCommand(context.Message.OrderId, OrderStatus.Delivered),
            context.CancellationToken);
    }
}

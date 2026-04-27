using Delivery.API.Application.Commands;
using Delivery.API.Application.Interfaces;
using FoodFleet.Shared.Events.Orders;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Delivery.API.Infrastructure.Consumers;

public class OrderReadyConsumer : IConsumer<OrderStatusChangedEvent>
{
    private readonly IDeliveryService _deliveryService;
    private readonly ILogger<OrderReadyConsumer> _logger;

    public OrderReadyConsumer(IDeliveryService deliveryService, ILogger<OrderReadyConsumer> logger)
    {
        _deliveryService = deliveryService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderStatusChangedEvent> context)
    {
        if (context.Message.NewStatus == "Ready")
        {
            _logger.LogInformation("Order {OrderId} is ready - assigning delivery agent", context.Message.OrderId);
            await _deliveryService.AssignAsync(new AssignDeliveryCommand(
                context.Message.OrderId,
                context.Message.CustomerId,
                context.Message.CustomerEmail),
                context.CancellationToken);
        }
    }
}

using Delivery.API.Application.Commands;
using Delivery.API.Application.Interfaces;
using FoodFleet.Shared.Events.Orders;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Delivery.API.Infrastructure.Consumers;

/// <summary>
/// MassTransit consumer that listens for <see cref="OrderStatusChangedEvent"/> messages
/// and automatically assigns a delivery agent when an order status changes to "Ready".
/// </summary>
public class OrderReadyConsumer : IConsumer<OrderStatusChangedEvent>
{
    private readonly IDeliveryService _deliveryService;
    private readonly ILogger<OrderReadyConsumer> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="OrderReadyConsumer"/>.
    /// </summary>
    /// <param name="deliveryService">The delivery service used to assign agents.</param>
    /// <param name="logger">The logger for diagnostic output.</param>
    public OrderReadyConsumer(IDeliveryService deliveryService, ILogger<OrderReadyConsumer> logger)
    {
        _deliveryService = deliveryService;
        _logger = logger;
    }

    /// <summary>
    /// Processes the incoming event. If the order status is "Ready", assigns an available
    /// delivery agent to the order.
    /// </summary>
    /// <param name="context">The consume context containing the event message.</param>
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

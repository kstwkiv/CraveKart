using FoodFleet.Shared.Events.Delivery;
using MassTransit;
using Microsoft.Extensions.Logging;
using Order.API.Application.Commands;
using Order.API.Application.Interfaces;
using Order.API.Domain.Enums;

namespace Order.API.Infrastructure.Consumers;

/// <summary>
/// MassTransit consumer that handles <see cref="DeliveryCompletedEvent"/> messages
/// and updates the associated order status to <see cref="OrderStatus.Delivered"/>.
/// </summary>
public class DeliveryCompletedConsumer : IConsumer<DeliveryCompletedEvent>
{
    private readonly IOrderService _orderService;
    private readonly ILogger<DeliveryCompletedConsumer> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="DeliveryCompletedConsumer"/>.
    /// </summary>
    /// <param name="orderService">The order service for updating order status.</param>
    /// <param name="logger">The logger for diagnostic output.</param>
    public DeliveryCompletedConsumer(IOrderService orderService, ILogger<DeliveryCompletedConsumer> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// Processes the delivery completed event and marks the order as Delivered.
    /// </summary>
    /// <param name="context">The consume context containing the event message.</param>
    public async Task Consume(ConsumeContext<DeliveryCompletedEvent> context)
    {
        _logger.LogInformation("Delivery completed for Order {OrderId}", context.Message.OrderId);
        await _orderService.UpdateStatusAsync(
            new UpdateOrderStatusCommand(context.Message.OrderId, OrderStatus.Delivered),
            context.CancellationToken);
    }
}

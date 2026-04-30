using FoodFleet.Shared.Events.Payments;
using MassTransit;
using Microsoft.Extensions.Logging;
using Order.API.Application.Commands;
using Order.API.Application.Interfaces;
using Order.API.Domain.Enums;

namespace Order.API.Infrastructure.Consumers;

/// <summary>
/// MassTransit consumer that handles <see cref="PaymentFailedEvent"/> messages
/// and updates the associated order status to <see cref="OrderStatus.Cancelled"/>.
/// </summary>
public class PaymentFailedConsumer : IConsumer<PaymentFailedEvent>
{
    private readonly IOrderService _orderService;
    private readonly ILogger<PaymentFailedConsumer> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="PaymentFailedConsumer"/>.
    /// </summary>
    /// <param name="orderService">The order service for updating order status.</param>
    /// <param name="logger">The logger for diagnostic output.</param>
    public PaymentFailedConsumer(IOrderService orderService, ILogger<PaymentFailedConsumer> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// Processes the payment failed event and cancels the associated order.
    /// </summary>
    /// <param name="context">The consume context containing the event message.</param>
    public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
    {
        _logger.LogInformation("Payment failed for Order {OrderId}", context.Message.OrderId);
        await _orderService.UpdateStatusAsync(
            new UpdateOrderStatusCommand(context.Message.OrderId, OrderStatus.Cancelled),
            context.CancellationToken);
    }
}

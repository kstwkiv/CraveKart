using FoodFleet.Shared.Events.Payments;
using MassTransit;
using Microsoft.Extensions.Logging;
using Order.API.Application.Commands;
using Order.API.Application.Interfaces;
using Order.API.Domain.Enums;

namespace Order.API.Infrastructure.Consumers;

/// <summary>
/// MassTransit consumer that handles <see cref="PaymentConfirmedEvent"/> messages
/// and updates the associated order status to <see cref="OrderStatus.Confirmed"/>.
/// </summary>
public class PaymentConfirmedConsumer : IConsumer<PaymentConfirmedEvent>
{
    private readonly IOrderService _orderService;
    private readonly ILogger<PaymentConfirmedConsumer> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="PaymentConfirmedConsumer"/>.
    /// </summary>
    /// <param name="orderService">The order service for updating order status.</param>
    /// <param name="logger">The logger for diagnostic output.</param>
    public PaymentConfirmedConsumer(IOrderService orderService, ILogger<PaymentConfirmedConsumer> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// Processes the payment confirmed event and marks the order as Confirmed.
    /// </summary>
    /// <param name="context">The consume context containing the event message.</param>
    public async Task Consume(ConsumeContext<PaymentConfirmedEvent> context)
    {
        _logger.LogInformation("Payment confirmed for Order {OrderId}", context.Message.OrderId);
        await _orderService.UpdateStatusAsync(
            new UpdateOrderStatusCommand(context.Message.OrderId, OrderStatus.Confirmed),
            context.CancellationToken);
    }
}

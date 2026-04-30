using FoodFleet.Shared.Events.Orders;
using MassTransit;
using Microsoft.Extensions.Logging;
using Payment.API.Application.Commands;
using Payment.API.Application.Interfaces;

namespace Payment.API.Infrastructure.Consumers;

/// <summary>
/// MassTransit consumer that handles <see cref="OrderPlacedEvent"/> messages
/// and automatically processes payment for the new order.
/// </summary>
public class OrderPlacedConsumer : IConsumer<OrderPlacedEvent>
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<OrderPlacedConsumer> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="OrderPlacedConsumer"/>.
    /// </summary>
    /// <param name="paymentService">The payment service for processing payments.</param>
    /// <param name="logger">The logger for diagnostic output.</param>
    public OrderPlacedConsumer(IPaymentService paymentService, ILogger<OrderPlacedConsumer> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    /// <summary>
    /// Processes the order placed event and initiates payment processing.
    /// </summary>
    /// <param name="context">The consume context containing the event message.</param>
    public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
    {
        _logger.LogInformation("Processing payment for Order {OrderId}", context.Message.OrderId);

        await _paymentService.ProcessAsync(new ProcessPaymentCommand(
            context.Message.OrderId,
            context.Message.CustomerId,
            context.Message.TotalAmount,
            context.Message.PaymentMethod),
            context.CancellationToken);
    }
}

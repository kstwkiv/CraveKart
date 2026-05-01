using FoodFleet.Shared.Events.Orders;
using MassTransit;
using Microsoft.Extensions.Logging;
using Payment.API.Application.Commands;
using Payment.API.Application.Interfaces;

namespace Payment.API.Infrastructure.Consumers;

/// <summary>
/// Handles <see cref="OrderPlacedEvent"/>.
/// - UpiNow  → creates a Confirmed payment immediately (triggers order confirmation).
/// - CashOnDelivery → creates a Pending payment; confirmed only after delivery.
/// </summary>
public class OrderPlacedConsumer : IConsumer<OrderPlacedEvent>
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<OrderPlacedConsumer> _logger;

    public OrderPlacedConsumer(IPaymentService paymentService, ILogger<OrderPlacedConsumer> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Order {OrderId} placed with payment method {Method}", msg.OrderId, msg.PaymentMethod);

        var isCod = msg.PaymentMethod.Equals("CashOnDelivery", StringComparison.OrdinalIgnoreCase);

        if (isCod)
        {
            // Create a Pending payment — will be confirmed when delivery completes
            await _paymentService.CreatePendingAsync(new ProcessPaymentCommand(
                msg.OrderId,
                msg.CustomerId,
                msg.CustomerEmail,
                msg.TotalAmount,
                msg.PaymentMethod),
                context.CancellationToken);

            _logger.LogInformation("COD order {OrderId} — payment pending until delivery.", msg.OrderId);
        }
        else
        {
            // UpiNow — confirm immediately, which triggers order → Confirmed
            await _paymentService.ProcessAsync(new ProcessPaymentCommand(
                msg.OrderId,
                msg.CustomerId,
                msg.CustomerEmail,
                msg.TotalAmount,
                msg.PaymentMethod),
                context.CancellationToken);

            _logger.LogInformation("UPI order {OrderId} — payment confirmed immediately.", msg.OrderId);
        }
    }
}

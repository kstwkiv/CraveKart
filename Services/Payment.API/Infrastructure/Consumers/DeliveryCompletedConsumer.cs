using FoodFleet.Shared.Events.Delivery;
using MassTransit;
using Microsoft.Extensions.Logging;
using Payment.API.Application.Interfaces;

namespace Payment.API.Infrastructure.Consumers;

/// <summary>
/// When a delivery completes, confirms any pending COD payment for that order.
/// This triggers a <c>PaymentConfirmedEvent</c> which moves the order to Confirmed/Delivered.
/// </summary>
public class DeliveryCompletedConsumer : IConsumer<DeliveryCompletedEvent>
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<DeliveryCompletedConsumer> _logger;

    public DeliveryCompletedConsumer(IPaymentService paymentService, ILogger<DeliveryCompletedConsumer> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<DeliveryCompletedEvent> context)
    {
        var orderId = context.Message.OrderId;
        _logger.LogInformation("Delivery completed for Order {OrderId} — confirming COD payment if pending.", orderId);

        var result = await _paymentService.ConfirmCodAsync(orderId, context.CancellationToken);

        if (result != null)
            _logger.LogInformation("COD payment for Order {OrderId} confirmed (status: {Status}).", orderId, result.Status);
        else
            _logger.LogWarning("No payment found for Order {OrderId} on delivery completion.", orderId);
    }
}

using FoodFleet.Shared.Events.Orders;
using MassTransit;
using Microsoft.Extensions.Logging;
using Payment.API.Application.Interfaces;
using Payment.API.Domain.Enums;
using FoodFleet.Shared.Events.Payments;
using FoodFleet.Shared.Messaging.Interfaces;

namespace Payment.API.Infrastructure.Consumers;

/// <summary>
/// Listens for <see cref="OrderCancelledEvent"/> and automatically refunds
/// the associated payment if it is in a Confirmed state.
/// </summary>
public class OrderCancelledConsumer : IConsumer<OrderCancelledEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<OrderCancelledConsumer> _logger;

    public OrderCancelledConsumer(
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher,
        ILogger<OrderCancelledConsumer> logger)
    {
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCancelledEvent> context)
    {
        var msg = context.Message;

        var payment = await _unitOfWork.Payments.GetByOrderIdAsync(msg.OrderId);

        if (payment == null)
        {
            _logger.LogWarning("No payment found for cancelled order {OrderId} — skipping refund.", msg.OrderId);
            return;
        }

        if (payment.Status != PaymentStatus.Confirmed)
        {
            _logger.LogInformation(
                "Payment {PaymentId} for order {OrderId} is already {Status} — skipping refund.",
                payment.Id, msg.OrderId, payment.Status);
            return;
        }

        payment.Status = PaymentStatus.Refunded;
        payment.ProcessedAt = IstClock.Now;
        _unitOfWork.Payments.Update(payment);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Refunded payment {PaymentId} for cancelled order {OrderId}.", payment.Id, msg.OrderId);

        await _eventPublisher.PublishAsync(new PaymentRefundedEvent
        {
            PaymentId = payment.Id,
            OrderId = payment.OrderId,
            CustomerId = payment.CustomerId,
            CustomerEmail = msg.CustomerEmail,
            Amount = payment.Amount,
            RefundedAt = IstClock.Now
        }, context.CancellationToken);
    }
}

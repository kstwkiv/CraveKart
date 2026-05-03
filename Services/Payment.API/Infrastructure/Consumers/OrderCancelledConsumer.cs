// 'using' — imports the shared Orders events namespace so OrderCancelledEvent is in scope
using FoodFleet.Shared.Events.Orders;
// 'using' — imports MassTransit; IConsumer<T> and ConsumeContext<T> are defined here
using MassTransit;
// 'using' — imports Microsoft's logging abstractions for structured, levelled logging
using Microsoft.Extensions.Logging;
// 'using' — imports the IUnitOfWork interface for transactional data access
using Payment.API.Application.Interfaces;
// 'using' — imports the PaymentStatus enum to compare payment states
using Payment.API.Domain.Enums;
// 'using' — imports the PaymentRefundedEvent domain event published after a successful refund
using FoodFleet.Shared.Events.Payments;
// 'using' — imports IEventPublisher, the abstraction for publishing domain events to the message bus
using FoodFleet.Shared.Messaging.Interfaces;

// 'namespace' — scopes this class to the Payment API's infrastructure consumer layer
namespace Payment.API.Infrastructure.Consumers;

/// <summary>
/// Listens for <see cref="OrderCancelledEvent"/> and automatically refunds
/// the associated payment if it is in a Confirmed state.
/// </summary>
// 'public' — access modifier: visible to MassTransit's consumer registration and the DI container
// 'class' — reference type grouping the consumer's dependencies and message-handling logic
// ': IConsumer<OrderCancelledEvent>' — interface implementation; MassTransit routes OrderCancelledEvent messages here
public class OrderCancelledConsumer : IConsumer<OrderCancelledEvent>
{
    // 'private readonly' — field is set once via constructor injection and cannot be reassigned
    private readonly IUnitOfWork _unitOfWork;
    // IEventPublisher — abstraction over the message bus; decouples this class from a specific broker
    private readonly IEventPublisher _eventPublisher;
    // ILogger<T> — generic logger scoped to this consumer class for categorised log output
    private readonly ILogger<OrderCancelledConsumer> _logger;

    // Constructor — the DI container calls this, injecting all three dependencies at runtime
    public OrderCancelledConsumer(
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher,
        ILogger<OrderCancelledConsumer> logger)
    {
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    // 'public' — required by the IConsumer<T> interface; MassTransit calls this method for each received message
    // 'async' — enables the use of 'await' inside; the compiler transforms this into a state machine
    // 'Task' — the async return type representing a unit of work with no result value
    // 'ConsumeContext<OrderCancelledEvent>' — MassTransit envelope providing the message and bus metadata
    public async Task Consume(ConsumeContext<OrderCancelledEvent> context)
    {
        // 'var' — implicitly typed local; compiler infers OrderCancelledEvent from context.Message
        var msg = context.Message;

        // 'await' — suspends execution until the async repository call completes
        var payment = await _unitOfWork.Payments.GetByOrderIdAsync(msg.OrderId);

        // 'if' — conditional branch; guards against processing when no payment record exists
        // 'null' — represents the absence of an object; checking prevents NullReferenceException
        if (payment == null)
        {
            _logger.LogWarning("No payment found for cancelled order {OrderId} — skipping refund.", msg.OrderId);
            // 'return' — early exit; stops further execution of this method
            return;
        }

        // Enum comparison — checks whether the payment's current state is not Confirmed
        // 'if' + early return — idiomatic guard clause to skip unnecessary work
        if (payment.Status != PaymentStatus.Confirmed)
        {
            _logger.LogInformation(
                "Payment {PaymentId} for order {OrderId} is already {Status} — skipping refund.",
                payment.Id, msg.OrderId, payment.Status);
            return;
        }

        // Mutating the entity's Status property to Refunded before persisting
        payment.Status = PaymentStatus.Refunded;
        payment.ProcessedAt = IstClock.Now;
        // Marks the entity as modified in the EF Core change tracker
        _unitOfWork.Payments.Update(payment);
        // 'await' — waits for the database transaction to commit
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Refunded payment {PaymentId} for cancelled order {OrderId}.", payment.Id, msg.OrderId);

        // 'await' — publishes the domain event asynchronously; other services (e.g. Notification) will react
        // 'new PaymentRefundedEvent { ... }' — object initialiser creates and populates the event in one expression
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

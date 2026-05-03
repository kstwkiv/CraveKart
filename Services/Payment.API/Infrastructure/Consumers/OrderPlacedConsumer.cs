// 'using' — imports the shared events namespace so OrderPlacedEvent is available without full qualification
using FoodFleet.Shared.Events.Orders;
// 'using' — imports MassTransit, the message-bus library; IConsumer<T> and ConsumeContext<T> live here
using MassTransit;
// 'using' — imports Microsoft's structured logging abstractions (ILogger<T>)
using Microsoft.Extensions.Logging;
// 'using' — imports the ProcessPaymentCommand record used to carry payment data into the service
using Payment.API.Application.Commands;
// 'using' — imports the IPaymentService interface so the consumer can call payment operations
using Payment.API.Application.Interfaces;

// 'namespace' — logical grouping that prevents name collisions across the solution
namespace Payment.API.Infrastructure.Consumers;

/// <summary>
/// Handles <see cref="OrderPlacedEvent"/>.
/// - UpiNow  → creates a Confirmed payment immediately (triggers order confirmation).
/// - CashOnDelivery → creates a Pending payment; confirmed only after delivery.
/// </summary>
// 'public' — access modifier: visible to MassTransit's DI scanning and any other assembly
// 'class' — reference type that groups state and behaviour
// 'IConsumer<OrderPlacedEvent>' — MassTransit contract; implementing this interface tells the bus to route
//   OrderPlacedEvent messages to this class's Consume method
public class OrderPlacedConsumer : IConsumer<OrderPlacedEvent>
{
    // 'private readonly' — dependency stored immutably; set once in the constructor via DI
    private readonly IPaymentService _paymentService;
    // ILogger<OrderPlacedConsumer> — generic logger scoped to this class for structured, categorised log output
    private readonly ILogger<OrderPlacedConsumer> _logger;

    // Constructor — receives dependencies injected by the DI container at runtime
    public OrderPlacedConsumer(IPaymentService paymentService, ILogger<OrderPlacedConsumer> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    // 'public' — must be public so MassTransit can invoke it via the IConsumer<T> interface
    // 'async' — method is asynchronous; the compiler generates a state machine to handle awaits
    // 'Task' — represents the ongoing async operation (no return value, analogous to void in async context)
    // 'ConsumeContext<OrderPlacedEvent>' — MassTransit wrapper around the message; provides the message payload and metadata
    public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
    {
        // 'var' — implicitly typed; compiler infers type OrderPlacedEvent from context.Message
        var msg = context.Message;
        // Structured log: {OrderId} and {Method} are named placeholders captured in the log record
        _logger.LogInformation("Order {OrderId} placed with payment method {Method}", msg.OrderId, msg.PaymentMethod);

        // 'bool' — a boolean type holding true or false; result of the string equality check
        // 'var' with bool — isCod is true when the payment method is "CashOnDelivery"
        var isCod = msg.PaymentMethod.Equals("CashOnDelivery", StringComparison.OrdinalIgnoreCase);

        // 'if' — conditional branch; selects the COD path or the UPI path
        if (isCod)
        {
            // Create a Pending payment — will be confirmed when delivery completes
            // 'await' — suspends this method until CreatePendingAsync completes, freeing the thread
            // 'new ProcessPaymentCommand(...)' — constructs the command value object with order details
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

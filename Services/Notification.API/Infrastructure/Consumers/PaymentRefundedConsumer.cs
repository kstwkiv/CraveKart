// 'using' — imports the shared Payments events namespace so PaymentRefundedEvent is in scope
using FoodFleet.Shared.Events.Payments;
// 'using' — imports MassTransit so IConsumer<> and ConsumeContext<> are available
using MassTransit;
// 'using' — imports the Notification application interfaces namespace (IEmailService)
using Notification.API.Application.Interfaces;
// 'using' — imports the EmailTemplates helper class for building email bodies
using Notification.API.Infrastructure.Services;

// 'namespace' — logical grouping that prevents name collisions across the solution
namespace Notification.API.Infrastructure.Consumers;

/// <summary>
/// Handles <see cref="PaymentRefundedEvent"/> and sends a refund confirmation email.
/// </summary>
// 'public' — access modifier: visible to MassTransit's consumer registration in DI
// 'class' — reference type; MassTransit creates a new instance per message consumed
// IConsumer<PaymentRefundedEvent> — MassTransit contract: this class handles PaymentRefundedEvent messages
public class PaymentRefundedConsumer : IConsumer<PaymentRefundedEvent>
{
    // 'private readonly' — encapsulated, immutable after construction; set via Dependency Injection
    private readonly IEmailService _emailService;

    // Constructor expression body (=>) — concise single-assignment constructor
    // 'public' — must be public for the DI container to instantiate this consumer
    public PaymentRefundedConsumer(IEmailService emailService) => _emailService = emailService;

    // 'public' — satisfies the IConsumer<PaymentRefundedEvent> interface contract
    // 'async' — enables await; the compiler generates a state machine for non-blocking execution
    // 'Task' — represents the in-flight async work with no result value (void equivalent for async)
    // ConsumeContext<PaymentRefundedEvent> — MassTransit wrapper providing the message and metadata
    public async Task Consume(ConsumeContext<PaymentRefundedEvent> context)
    {
        // 'var' — compiler infers PaymentRefundedEvent; context.Message is the deserialized event payload
        var msg = context.Message;
        // 'if' — guards against sending email when the address is null or empty
        // string.IsNullOrEmpty — returns true if the string is null or has zero characters
        // 'return' — exits the method early without sending an email
        if (string.IsNullOrEmpty(msg.CustomerEmail)) return;
        // 'await' — suspends execution until the email is dispatched, freeing the thread
        await _emailService.SendAsync(
            msg.CustomerEmail,                                    // 'string' — recipient email address
            "Your Refund Has Been Processed 💸",                  // 'string' — email subject line literal
            // EmailTemplates.PaymentRefunded — static helper that builds the HTML email body
            // ToString() — converts Guid to its standard string representation
            EmailTemplates.PaymentRefunded(msg.OrderId.ToString(), msg.Amount, msg.RefundedAt.ToString("f")));
            // "f" format specifier — formats DateTime as "full date/time pattern (short time)"
    }
}

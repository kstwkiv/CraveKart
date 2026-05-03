// 'using' — imports the FoodFleet.Shared.Events namespace so IstClock is in scope
using FoodFleet.Shared.Events;

// 'namespace' — logical grouping that prevents name collisions; shared across multiple microservices
namespace FoodFleet.Shared.Events.Payments;

/// <summary>
/// Domain event published when a payment is successfully refunded.
/// Consumed by the Notification service to send a refund confirmation email to the customer.
/// </summary>
// 'public' — access modifier: this event class must be visible to all consuming microservices
// 'class' — reference type; the message broker serializes/deserializes instances across service boundaries
public class PaymentRefundedEvent
{
    /// <summary>Gets or sets the unique identifier of the payment record.</summary>
    // 'public' — property must be public for JSON serialization by the message broker
    // 'Guid' — 128-bit globally unique identifier; used as the payment's primary key
    public Guid PaymentId { get; set; }

    /// <summary>Gets or sets the unique identifier of the associated order.</summary>
    // 'Guid' — foreign key correlating this event to the order in the Order bounded context
    public Guid OrderId { get; set; }

    /// <summary>Gets or sets the unique identifier of the customer receiving the refund.</summary>
    // 'Guid' — foreign key correlating this event to the customer in the Identity bounded context
    public Guid CustomerId { get; set; }

    /// <summary>Gets or sets the email address of the customer for refund notifications.</summary>
    // 'string' — built-in Unicode string type; carries the recipient address for the refund email
    // = string.Empty — default initialiser; ensures the property is never null
    public string CustomerEmail { get; set; } = string.Empty;

    /// <summary>Gets or sets the refunded amount.</summary>
    // 'decimal' — 128-bit fixed-point numeric type; used for monetary values to avoid floating-point rounding errors
    public decimal Amount { get; set; }

    /// <summary>Gets or sets the currency code. Defaults to "INR".</summary>
    // 'string' — non-nullable; = "INR" sets the default currency to Indian Rupee
    public string Currency { get; set; } = "INR";

    /// <summary>Gets or sets the IST timestamp when the refund was processed.</summary>
    // 'DateTime' — value type representing a point in time; IstClock.Now provides the IST-adjusted value
    public DateTime RefundedAt { get; set; } = IstClock.Now;
}

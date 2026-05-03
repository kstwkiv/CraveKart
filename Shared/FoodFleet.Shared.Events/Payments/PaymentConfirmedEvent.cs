// 'using' — imports the FoodFleet.Shared.Events namespace so IstClock is in scope
using FoodFleet.Shared.Events;

// 'namespace' — logical grouping that prevents name collisions; shared across multiple microservices
namespace FoodFleet.Shared.Events.Payments;

/// <summary>
/// Domain event published when a payment is successfully confirmed.
/// Consumed by the Order service (to mark order as Confirmed) and the Notification service (to send a payment confirmation email).
/// </summary>
// 'public' — access modifier: this event class must be visible to all consuming microservices
// 'class' — reference type; the message broker serializes/deserializes instances across service boundaries
public class PaymentConfirmedEvent
{
    /// <summary>Gets or sets the unique identifier of the payment record.</summary>
    // 'public' — property must be public for JSON serialization by the message broker
    // 'Guid' — 128-bit globally unique identifier; used as the payment's primary key
    public Guid PaymentId { get; set; }

    /// <summary>Gets or sets the unique identifier of the associated order.</summary>
    // 'Guid' — foreign key correlating this event to the order in the Order bounded context
    public Guid OrderId { get; set; }

    /// <summary>Gets or sets the unique identifier of the customer who made the payment.</summary>
    // 'Guid' — foreign key correlating this event to the customer in the Identity bounded context
    public Guid CustomerId { get; set; }

    /// <summary>Gets or sets the email address of the customer for payment notifications.</summary>
    // 'string' — built-in Unicode string type; carries the recipient address for the confirmation email
    // = string.Empty — default initialiser; ensures the property is never null
    public string CustomerEmail { get; set; } = string.Empty;

    /// <summary>Gets or sets the total amount confirmed.</summary>
    // 'decimal' — 128-bit fixed-point numeric type; used for monetary values to avoid floating-point rounding errors
    public decimal Amount { get; set; }

    /// <summary>Gets or sets the currency code. Defaults to "INR".</summary>
    // 'string' — non-nullable; = "INR" sets the default currency to Indian Rupee
    public string Currency { get; set; } = "INR";

    /// <summary>Gets or sets the payment method used (e.g., "UpiNow", "CashOnDelivery").</summary>
    // 'string' — non-nullable; = string.Empty prevents null reference issues in consumers
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>Gets or sets the IST timestamp when the payment was confirmed.</summary>
    // 'DateTime' — value type representing a point in time; IstClock.Now provides the IST-adjusted value
    public DateTime ConfirmedAt { get; set; } = IstClock.Now;
}

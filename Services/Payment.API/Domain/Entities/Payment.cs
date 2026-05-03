// 'using' — imports the Payment domain enums namespace so PaymentStatus is in scope
using Payment.API.Domain.Enums;

// 'namespace' — logical grouping that prevents name collisions; mirrors the folder structure
namespace Payment.API.Domain.Entities;

/// <summary>
/// Represents a payment transaction record in the Payment bounded context.
/// </summary>
// 'public' — access modifier: this class is visible to other layers (Application, Infrastructure)
// 'class' — reference type; each Payment instance is a separate object on the managed heap
public class Payment
{
    /// <summary>Gets or sets the unique identifier of the payment record.</summary>
    // 'public' — property is accessible to EF Core (for mapping) and other layers
    // 'Guid' — a 128-bit globally unique identifier; used as the primary key to avoid sequential ID guessing
    // Guid.NewGuid() — generates a new random GUID as the default value when a Payment is created
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Gets or sets the unique identifier of the associated order.</summary>
    // 'Guid' — foreign key linking this payment to an order in the Order bounded context
    public Guid OrderId { get; set; }

    /// <summary>Gets or sets the unique identifier of the customer who made the payment.</summary>
    // 'Guid' — foreign key linking this payment to a customer in the Identity bounded context
    public Guid CustomerId { get; set; }

    /// <summary>Gets or sets the total amount charged.</summary>
    // 'decimal' — 128-bit fixed-point numeric type; preferred for monetary values because it avoids
    //             the floating-point rounding errors that 'double' or 'float' would introduce
    public decimal Amount { get; set; }

    /// <summary>Gets or sets the currency code for the payment. Defaults to "INR".</summary>
    // 'string' — built-in reference type representing a sequence of Unicode characters
    // = "INR" — default initialiser; ensures the property is never null and defaults to Indian Rupee
    public string Currency { get; set; } = "INR";

    /// <summary>Gets or sets the current status of the payment.</summary>
    // PaymentStatus — an enum (value type) that restricts the status to a known set of values
    // = PaymentStatus.Pending — default value; every new payment starts in the Pending state
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    /// <summary>Gets or sets the Stripe payment intent ID for card payments. Null for cash orders.</summary>
    // 'string?' — nullable reference type (the '?' suffix); the value may legitimately be null
    //             (e.g., cash-on-delivery orders have no Stripe intent)
    // 'null' — the absence of an object reference; valid here because not all payments use Stripe
    public string? StripePaymentIntentId { get; set; }

    /// <summary>Gets or sets the payment method used (e.g., "Card", "CashOnDelivery").</summary>
    // 'string' — non-nullable; = string.Empty ensures the property is initialised to "" not null
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>Gets or sets the customer's email for refund notifications.</summary>
    // 'string' — non-nullable; = string.Empty prevents null reference issues downstream
    public string CustomerEmail { get; set; } = string.Empty;

    /// <summary>Gets or sets the IST timestamp when the payment record was created.</summary>
    // 'DateTime' — value type representing a point in time; IstClock.Now provides the IST-adjusted value
    public DateTime CreatedAt { get; set; } = IstClock.Now;

    /// <summary>Gets or sets the UTC timestamp when the payment was processed.</summary>
    // 'DateTime' — value type; set when the payment transitions out of the Pending state
    public DateTime ProcessedAt { get; set; }
}

using Payment.API.Domain.Enums;

namespace Payment.API.Domain.Entities;

/// <summary>
/// Represents a payment transaction record in the Payment bounded context.
/// </summary>
public class Payment
{
    /// <summary>Gets or sets the unique identifier of the payment record.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Gets or sets the unique identifier of the associated order.</summary>
    public Guid OrderId { get; set; }

    /// <summary>Gets or sets the unique identifier of the customer who made the payment.</summary>
    public Guid CustomerId { get; set; }

    /// <summary>Gets or sets the total amount charged.</summary>
    public decimal Amount { get; set; }

    /// <summary>Gets or sets the currency code for the payment. Defaults to "INR".</summary>
    public string Currency { get; set; } = "INR";

    /// <summary>Gets or sets the current status of the payment.</summary>
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    /// <summary>Gets or sets the Stripe payment intent ID for card payments. Null for cash orders.</summary>
    public string? StripePaymentIntentId { get; set; }

    /// <summary>Gets or sets the payment method used (e.g., "Card", "CashOnDelivery").</summary>
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>Gets or sets the customer's email for refund notifications.</summary>
    public string CustomerEmail { get; set; } = string.Empty;

    /// <summary>Gets or sets the IST timestamp when the payment record was created.</summary>
    public DateTime CreatedAt { get; set; } = IstClock.Now;

    /// <summary>Gets or sets the UTC timestamp when the payment was processed.</summary>
    public DateTime ProcessedAt { get; set; }
}
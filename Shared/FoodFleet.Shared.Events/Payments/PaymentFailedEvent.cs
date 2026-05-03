using FoodFleet.Shared.Events;

namespace FoodFleet.Shared.Events.Payments;

/// <summary>
/// Domain event published when a payment fails to process.
/// Consumed by the Order service (to cancel the order) and the Notification service (to send a failure notification email).
/// </summary>
public class PaymentFailedEvent
{
    /// <summary>Gets or sets the unique identifier of the associated order.</summary>
    public Guid OrderId { get; set; }

    /// <summary>Gets or sets the unique identifier of the customer.</summary>
    public Guid CustomerId { get; set; }

    /// <summary>Gets or sets the email address of the customer for failure notifications.</summary>
    public string CustomerEmail { get; set; } = string.Empty;

    /// <summary>Gets or sets the reason the payment failed.</summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>Gets or sets the IST timestamp when the payment failure occurred.</summary>
    public DateTime FailedAt { get; set; } = IstClock.Now;
}
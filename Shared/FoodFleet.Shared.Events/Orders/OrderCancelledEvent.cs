using FoodFleet.Shared.Events;

namespace FoodFleet.Shared.Events.Orders;

/// <summary>
/// Domain event published when a customer cancels an order.
/// Consumed by the Payment service (to trigger a refund) and the Notification service (to send a cancellation email).
/// </summary>
public class OrderCancelledEvent
{
    /// <summary>Gets or sets the unique identifier of the cancelled order.</summary>
    public Guid OrderId { get; set; }

    /// <summary>Gets or sets the unique identifier of the customer who cancelled the order.</summary>
    public Guid CustomerId { get; set; }

    /// <summary>Gets or sets the email address of the customer for cancellation notifications.</summary>
    public string CustomerEmail { get; set; } = string.Empty;

    /// <summary>Gets or sets the reason for the cancellation.</summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>Gets or sets the IST timestamp when the order was cancelled.</summary>
    public DateTime CancelledAt { get; set; } = IstClock.Now;
}
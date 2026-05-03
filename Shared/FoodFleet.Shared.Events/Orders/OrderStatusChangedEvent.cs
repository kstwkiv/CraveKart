// 'using' — imports the FoodFleet.Shared.Events namespace (provides IstClock)
using FoodFleet.Shared.Events;

// 'namespace' — scopes this event under the Orders sub-domain to keep related events together
namespace FoodFleet.Shared.Events.Orders;

/// <summary>
/// Domain event published when an order's status changes (e.g., Confirmed → Preparing → Ready).
/// Consumed by the Delivery service (to assign an agent when Ready) and the Notification service (to send status update emails).
/// </summary>
// 'public' — event class is accessible to all microservices that subscribe to order status changes
// 'class' — reference type; serialised and transported across service boundaries via the message bus
public class OrderStatusChangedEvent
{
    /// <summary>Gets or sets the unique identifier of the order whose status changed.</summary>
    // 'Guid' — globally unique identifier; correlates this status-change event to a specific order
    // '{ get; set; }' — auto-implemented property; compiler generates a hidden backing field
    public Guid OrderId { get; set; }

    /// <summary>Gets or sets the unique identifier of the customer who placed the order.</summary>
    // 'Guid' — identifies the customer so the Notification service can target the right user
    public Guid CustomerId { get; set; }

    /// <summary>Gets or sets the email address of the customer for status update notifications.</summary>
    // 'string' — customer's email address; used to send status-update notifications
    // '= string.Empty' — non-null default prevents null reference exceptions on deserialisation
    public string CustomerEmail { get; set; } = string.Empty;

    /// <summary>Gets or sets the previous status of the order.</summary>
    // 'string' — the status the order was in before this transition (e.g., "Pending")
    public string OldStatus { get; set; } = string.Empty;

    /// <summary>Gets or sets the new status of the order.</summary>
    // 'string' — the status the order has transitioned to (e.g., "Confirmed", "OutForDelivery")
    public string NewStatus { get; set; } = string.Empty;

    /// <summary>Gets or sets the IST timestamp when the status change occurred.</summary>
    // 'DateTime' — value type capturing the exact moment the status changed
    // 'IstClock.Now' — returns the current IST timestamp for consistent timezone handling
    public DateTime ChangedAt { get; set; } = IstClock.Now;
}

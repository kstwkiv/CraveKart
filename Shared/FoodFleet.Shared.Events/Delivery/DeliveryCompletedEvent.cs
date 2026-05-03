// 'using' — imports the FoodFleet.Shared.Events namespace (provides IstClock)
using FoodFleet.Shared.Events;

// 'namespace' — scopes this event under the Delivery sub-domain to avoid name collisions
namespace FoodFleet.Shared.Events.Delivery;

/// <summary>
/// Domain event published when a delivery is successfully completed.
/// Consumed by the Order service (to mark order as Delivered), the Payment service (to confirm COD payments),
/// and the Notification service (to send a delivery completion email).
/// </summary>
// 'public' — event class is accessible to all microservices that subscribe to this message
// 'class' — reference type; serialised and transported across service boundaries via the message bus
public class DeliveryCompletedEvent
{
    // 'public' — accessible to any consumer
    // 'Guid' — globally unique identifier; links this event back to the originating order
    // '{ get; set; }' — auto-property: compiler generates a private backing field automatically
    public Guid OrderId { get; set; }

    /// <summary>Gets or sets the unique identifier of the delivery agent who completed the delivery.</summary>
    // 'Guid' — identifies the delivery agent who completed the delivery
    public Guid AgentId { get; set; }

    /// <summary>Gets or sets the unique identifier of the customer who received the order.</summary>
    // 'Guid' — identifies the customer who placed the order
    public Guid CustomerId { get; set; }

    /// <summary>Gets or sets the email address of the customer for delivery notifications.</summary>
    // 'string' — text type for the customer's email address
    // '= string.Empty' — default initialiser prevents null reference exceptions on unset properties
    public string CustomerEmail { get; set; } = string.Empty;

    /// <summary>Gets or sets the IST timestamp when the delivery was completed.</summary>
    // 'DateTime' — value type capturing the exact moment the delivery was marked complete
    // 'IstClock.Now' — returns current IST timestamp; ensures consistent timezone across services
    public DateTime CompletedAt { get; set; } = IstClock.Now;
}

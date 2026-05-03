using FoodFleet.Shared.Events;

namespace FoodFleet.Shared.Events.Delivery;

/// <summary>
/// Domain event published when a delivery agent is assigned to an order.
/// Consumed by the Notification service to inform the customer of the assignment.
/// </summary>
public class DeliveryAssignedEvent
{
    /// <summary>Gets or sets the unique identifier of the order being delivered.</summary>
    public Guid OrderId { get; set; }

    /// <summary>Gets or sets the unique identifier of the assigned delivery agent.</summary>
    public Guid AgentId { get; set; }

    /// <summary>Gets or sets the full name of the assigned delivery agent.</summary>
    public string AgentName { get; set; } = string.Empty;

    /// <summary>Gets or sets the unique identifier of the customer who placed the order.</summary>
    public Guid CustomerId { get; set; }

    /// <summary>Gets or sets the email address of the customer for delivery notifications.</summary>
    public string CustomerEmail { get; set; } = string.Empty;

    /// <summary>Gets or sets the IST timestamp when the delivery was assigned.</summary>
    public DateTime AssignedAt { get; set; } = IstClock.Now;
}
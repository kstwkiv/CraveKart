namespace Delivery.API.Domain.Entities;

/// <summary>
/// Represents a delivery record tracking the assignment and progress of an order delivery.
/// </summary>
public class Delivery
{
    /// <summary>Gets or sets the unique identifier of the delivery record.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Gets or sets the unique identifier of the associated order.</summary>
    public Guid OrderId { get; set; }

    /// <summary>Gets or sets the unique identifier of the assigned delivery agent.</summary>
    public Guid AgentId { get; set; }

    /// <summary>Gets or sets the unique identifier of the customer who placed the order.</summary>
    public Guid CustomerId { get; set; }

    /// <summary>Gets or sets the email address of the customer for delivery notifications.</summary>
    public string CustomerEmail { get; set; } = string.Empty;

    /// <summary>Gets or sets the current delivery status (e.g., "Assigned", "Delivered").</summary>
    public string Status { get; set; } = "Assigned";

    /// <summary>Gets or sets the current latitude of the delivery agent. Null if not yet updated.</summary>
    public double? CurrentLat { get; set; }

    /// <summary>Gets or sets the current longitude of the delivery agent. Null if not yet updated.</summary>
    public double? CurrentLng { get; set; }

    /// <summary>Gets or sets the IST timestamp when the delivery was assigned.</summary>
    public DateTime AssignedAt { get; set; } = IstClock.Now;

    /// <summary>Gets or sets the UTC timestamp when the delivery was completed. Null if still in progress.</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>Gets or sets the delivery agent assigned to this delivery (navigation property).</summary>
    public DeliveryAgent Agent { get; set; } = null!;
}
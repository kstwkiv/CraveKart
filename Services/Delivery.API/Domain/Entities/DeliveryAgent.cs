namespace Delivery.API.Domain.Entities;

/// <summary>
/// Represents a registered delivery agent profile within the system.
/// </summary>
public class DeliveryAgent
{
    /// <summary>Gets or sets the unique identifier of the agent profile.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Gets or sets the user account ID linked to this agent profile.</summary>
    public Guid UserId { get; set; }

    /// <summary>Gets or sets the full name of the delivery agent.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Gets or sets the type of vehicle used by the agent (e.g., "Bike", "Car").</summary>
    public string VehicleType { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether the agent is currently available for new deliveries.</summary>
    public bool IsAvailable { get; set; } = true;

    /// <summary>Gets or sets the current latitude of the agent's standing location. Null if not set.</summary>
    public double? CurrentLat { get; set; }

    /// <summary>Gets or sets the current longitude of the agent's standing location. Null if not set.</summary>
    public double? CurrentLng { get; set; }

    /// <summary>Gets or sets the total number of deliveries completed by this agent.</summary>
    public int TotalDeliveries { get; set; } = 0;

    /// <summary>Gets or sets the total earnings accumulated by this agent (₹100 per delivery).</summary>
    public decimal TotalEarnings { get; set; } = 0;

    /// <summary>Gets or sets the IST timestamp when the agent profile was created.</summary>
    public DateTime CreatedAt { get; set; } = IstClock.Now;
}
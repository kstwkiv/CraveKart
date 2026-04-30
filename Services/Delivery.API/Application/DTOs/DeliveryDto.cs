namespace Delivery.API.Application.DTOs;

/// <summary>
/// Data transfer object representing a delivery record returned to API consumers.
/// </summary>
public class DeliveryDto
{
    /// <summary>Gets or sets the unique identifier of the delivery record.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the unique identifier of the associated order.</summary>
    public Guid OrderId { get; set; }

    /// <summary>Gets or sets the unique identifier of the assigned delivery agent.</summary>
    public Guid AgentId { get; set; }

    /// <summary>Gets or sets the current delivery status (e.g., "Assigned", "Delivered").</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Gets or sets the current latitude of the delivery agent. Null if not yet updated.</summary>
    public double? CurrentLat { get; set; }

    /// <summary>Gets or sets the current longitude of the delivery agent. Null if not yet updated.</summary>
    public double? CurrentLng { get; set; }

    /// <summary>Gets or sets the UTC timestamp when the delivery was assigned.</summary>
    public DateTime AssignedAt { get; set; }

    /// <summary>Gets or sets the UTC timestamp when the delivery was completed. Null if still in progress.</summary>
    public DateTime? CompletedAt { get; set; }
}
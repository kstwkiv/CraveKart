namespace Delivery.API.Domain.Entities;

public class Delivery
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public Guid AgentId { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public string Status { get; set; } = "Assigned";
    public double? CurrentLat { get; set; }
    public double? CurrentLng { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public DeliveryAgent Agent { get; set; } = null!;
}
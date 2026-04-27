namespace Delivery.API.Domain.Entities;

public class DeliveryAgent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
    public double? CurrentLat { get; set; }
    public double? CurrentLng { get; set; }
    public int TotalDeliveries { get; set; } = 0;
    public decimal TotalEarnings { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
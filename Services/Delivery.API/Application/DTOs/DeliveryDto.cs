namespace Delivery.API.Application.DTOs;

public class DeliveryDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid AgentId { get; set; }
    public string Status { get; set; } = string.Empty;
    public double? CurrentLat { get; set; }
    public double? CurrentLng { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
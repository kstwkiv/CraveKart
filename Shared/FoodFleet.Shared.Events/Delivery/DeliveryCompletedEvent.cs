namespace FoodFleet.Shared.Events.Delivery;

public class DeliveryCompletedEvent
{
    public Guid OrderId { get; set; }
    public Guid AgentId { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}
namespace FoodFleet.Shared.Events.Restaurants;

public class RestaurantRejectedEvent
{
    public Guid RestaurantId { get; set; }
    public Guid OwnerId { get; set; }
    public string OwnerEmail { get; set; } = string.Empty;
    public string RestaurantName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime RejectedAt { get; set; } = DateTime.UtcNow;
}

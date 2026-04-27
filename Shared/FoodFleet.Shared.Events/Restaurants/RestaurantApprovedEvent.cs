namespace FoodFleet.Shared.Events.Restaurants;

public class RestaurantApprovedEvent
{
    public Guid RestaurantId { get; set; }
    public Guid OwnerId { get; set; }
    public string OwnerEmail { get; set; } = string.Empty;
    public string RestaurantName { get; set; } = string.Empty;
    public DateTime ApprovedAt { get; set; } = DateTime.UtcNow;
}
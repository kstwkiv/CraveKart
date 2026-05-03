using FoodFleet.Shared.Events;

namespace FoodFleet.Shared.Events.Restaurants;

/// <summary>
/// Domain event published when an admin approves a restaurant application.
/// Consumed by the Notification service to send a congratulatory email to the restaurant owner.
/// </summary>
public class RestaurantApprovedEvent
{
    /// <summary>Gets or sets the unique identifier of the approved restaurant.</summary>
    public Guid RestaurantId { get; set; }

    /// <summary>Gets or sets the unique identifier of the restaurant owner.</summary>
    public Guid OwnerId { get; set; }

    /// <summary>Gets or sets the email address of the restaurant owner for approval notifications.</summary>
    public string OwnerEmail { get; set; } = string.Empty;

    /// <summary>Gets or sets the display name of the approved restaurant.</summary>
    public string RestaurantName { get; set; } = string.Empty;

    /// <summary>Gets or sets the IST timestamp when the restaurant was approved.</summary>
    public DateTime ApprovedAt { get; set; } = IstClock.Now;
}
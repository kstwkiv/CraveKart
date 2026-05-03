using FoodFleet.Shared.Events;

namespace FoodFleet.Shared.Events.Restaurants;

/// <summary>
/// Domain event published when an admin rejects or suspends a restaurant.
/// Consumed by the Notification service to send a rejection or suspension email to the restaurant owner.
/// </summary>
public class RestaurantRejectedEvent
{
    /// <summary>Gets or sets the unique identifier of the rejected or suspended restaurant.</summary>
    public Guid RestaurantId { get; set; }

    /// <summary>Gets or sets the unique identifier of the restaurant owner.</summary>
    public Guid OwnerId { get; set; }

    /// <summary>Gets or sets the email address of the restaurant owner for rejection notifications.</summary>
    public string OwnerEmail { get; set; } = string.Empty;

    /// <summary>Gets or sets the display name of the restaurant.</summary>
    public string RestaurantName { get; set; } = string.Empty;

    /// <summary>Gets or sets the reason for rejection or suspension. Prefix with "Suspended: " for suspensions.</summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>Gets or sets the IST timestamp when the rejection or suspension occurred.</summary>
    public DateTime RejectedAt { get; set; } = IstClock.Now;
}

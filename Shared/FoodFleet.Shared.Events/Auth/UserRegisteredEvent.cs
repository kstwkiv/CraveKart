using FoodFleet.Shared.Events;

namespace FoodFleet.Shared.Events.Auth;

/// <summary>
/// Domain event published when a new user account is successfully registered.
/// Consumed by the Notification service to send a welcome email and by the Restaurant service for audit logging.
/// </summary>
public class UserRegisteredEvent
{
    /// <summary>Gets or sets the unique identifier of the newly registered user.</summary>
    public Guid UserId { get; set; }

    /// <summary>Gets or sets the full name of the user.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Gets or sets the email address of the user.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the role assigned to the user (e.g., "Customer", "RestaurantOwner").</summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>Gets or sets the IST timestamp when the registration occurred.</summary>
    public DateTime RegisteredAt { get; set; } = IstClock.Now;
}
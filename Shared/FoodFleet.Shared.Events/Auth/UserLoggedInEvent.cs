// 'using' — imports the FoodFleet.Shared.Events namespace (provides IstClock and shared utilities)
using FoodFleet.Shared.Events;

// 'namespace' — groups this event type under the Auth sub-namespace to keep events organised by domain
namespace FoodFleet.Shared.Events.Auth;

/// <summary>
/// Domain event published when a user successfully logs in to the platform.
/// Consumed by the Notification service to send a sign-in security alert email.
/// </summary>
// 'public' — the event class is visible to all consumers across microservices
// 'class' — defines a reference type; event objects are passed by reference through the message bus
public class UserLoggedInEvent
{
    // 'public' — property is readable and writable by any consumer of this event
    // 'Guid' — 128-bit globally unique identifier; used as a collision-resistant, database-friendly ID
    // '{ get; set; }' — auto-implemented property: compiler generates a hidden backing field
    public Guid UserId { get; set; }

    /// <summary>Gets or sets the full name of the user.</summary>
    // 'string' — built-in reference type for immutable Unicode text
    // '= string.Empty' — initialises to an empty string so the property is never null by default
    public string FullName { get; set; } = string.Empty;

    /// <summary>Gets or sets the email address of the user.</summary>
    // 'string' — stores the user's email address as text
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the IST timestamp when the login occurred.</summary>
    // 'DateTime' — value type representing a point in time (date + time of day)
    // 'IstClock.Now' — custom clock helper that returns the current time in IST (Indian Standard Time)
    public DateTime LoggedInAt { get; set; } = IstClock.Now;
}

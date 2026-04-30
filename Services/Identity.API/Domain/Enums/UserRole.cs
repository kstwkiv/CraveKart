namespace Identity.API.Domain.Enums;

/// <summary>
/// Defines the roles available to user accounts in the CraveKart platform.
/// </summary>
public enum UserRole
{
    /// <summary>A regular customer who places food orders.</summary>
    Customer,

    /// <summary>A restaurant owner who manages restaurant listings and menus.</summary>
    RestaurantOwner,

    /// <summary>A delivery agent who picks up and delivers orders.</summary>
    DeliveryAgent,

    /// <summary>A platform administrator with full access to all management features.</summary>
    Admin
}
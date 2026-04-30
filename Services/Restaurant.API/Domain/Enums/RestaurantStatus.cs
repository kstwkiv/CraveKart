namespace Restaurant.API.Domain.Enums;

/// <summary>
/// Defines the approval lifecycle statuses of a restaurant on the platform.
/// </summary>
public enum RestaurantStatus
{
    /// <summary>The restaurant has been submitted and is awaiting admin review.</summary>
    Pending,

    /// <summary>The restaurant has been approved and is visible to customers.</summary>
    Active,

    /// <summary>The restaurant application was rejected by an admin.</summary>
    Rejected,

    /// <summary>The restaurant has been temporarily suspended by an admin.</summary>
    Suspended
}
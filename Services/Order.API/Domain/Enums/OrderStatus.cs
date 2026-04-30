namespace Order.API.Domain.Enums;

/// <summary>
/// Defines the lifecycle statuses of a food order.
/// </summary>
public enum OrderStatus
{
    /// <summary>The order has been placed by the customer and is awaiting payment confirmation.</summary>
    Placed,

    /// <summary>Payment has been confirmed and the restaurant has accepted the order.</summary>
    Confirmed,

    /// <summary>The restaurant is currently preparing the order.</summary>
    Preparing,

    /// <summary>The order is packed and ready for pickup by a delivery agent.</summary>
    Ready,

    /// <summary>A delivery agent has picked up the order and is en route.</summary>
    PickedUp,

    /// <summary>The order has been successfully delivered to the customer.</summary>
    Delivered,

    /// <summary>The order was cancelled by the customer or admin.</summary>
    Cancelled,

    /// <summary>The order was rejected by the restaurant.</summary>
    Rejected
}
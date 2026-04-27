namespace Order.API.Domain.Enums;

public enum OrderStatus
{
    Placed,
    Confirmed,
    Preparing,
    Ready,
    PickedUp,
    Delivered,
    Cancelled,
    Rejected
}
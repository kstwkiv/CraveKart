namespace Order.API.Domain.Enums;

/// <summary>
/// Defines the payment methods available when placing an order.
/// </summary>
public enum PaymentMethod
{
    /// <summary>Pay now via UPI — payment confirmed immediately.</summary>
    UpiNow,

    /// <summary>Pay in cash when the order is delivered.</summary>
    CashOnDelivery
}
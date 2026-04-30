namespace Order.API.Domain.Enums;

/// <summary>
/// Defines the payment methods available when placing an order.
/// </summary>
public enum PaymentMethod
{
    /// <summary>Payment by credit or debit card (processed online).</summary>
    Card,

    /// <summary>Payment in cash upon delivery.</summary>
    CashOnDelivery
}
namespace Payment.API.Domain.Enums;

/// <summary>
/// Defines the lifecycle statuses of a payment transaction.
/// </summary>
public enum PaymentStatus
{
    /// <summary>The payment has been initiated but not yet processed.</summary>
    Pending,

    /// <summary>The payment was successfully processed and confirmed.</summary>
    Confirmed,

    /// <summary>The payment processing failed.</summary>
    Failed,

    /// <summary>The payment was refunded to the customer.</summary>
    Refunded
}
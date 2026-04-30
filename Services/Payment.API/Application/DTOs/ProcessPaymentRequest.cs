namespace Payment.API.Application.DTOs;

/// <summary>
/// Request DTO for manually processing a payment via the API.
/// </summary>
public class ProcessPaymentRequest
{
    /// <summary>Gets or sets the unique identifier of the order to pay for.</summary>
    public Guid OrderId { get; set; }

    /// <summary>Gets or sets the total amount to charge.</summary>
    public decimal Amount { get; set; }

    /// <summary>Gets or sets the payment method (e.g., "Card", "CashOnDelivery").</summary>
    public string PaymentMethod { get; set; } = string.Empty;
}

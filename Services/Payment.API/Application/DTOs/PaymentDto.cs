namespace Payment.API.Application.DTOs;

/// <summary>
/// Data transfer object representing a payment record returned to API consumers.
/// </summary>
public class PaymentDto
{
    /// <summary>Gets or sets the unique identifier of the payment record.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the unique identifier of the associated order.</summary>
    public Guid OrderId { get; set; }

    /// <summary>Gets or sets the unique identifier of the customer who made the payment.</summary>
    public Guid CustomerId { get; set; }

    /// <summary>Gets or sets the total amount charged.</summary>
    public decimal Amount { get; set; }

    /// <summary>Gets or sets the currency code. Defaults to "INR".</summary>
    public string Currency { get; set; } = "INR";

    /// <summary>Gets or sets the current status of the payment (e.g., "Pending", "Confirmed", "Refunded").</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Gets or sets the payment method used (e.g., "UpiNow", "CashOnDelivery").</summary>
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>Gets or sets the IST timestamp when the payment record was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Gets or sets the IST timestamp when the payment was processed or last updated.</summary>
    public DateTime ProcessedAt { get; set; }
}
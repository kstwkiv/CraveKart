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

    /// <summary>Gets or sets the total amount charged.</summary>
    public decimal Amount { get; set; }

    /// <summary>Gets or sets the current payment status (e.g., "Confirmed", "Failed").</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Gets or sets the payment method used (e.g., "Card", "CashOnDelivery").</summary>
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>Gets or sets the UTC timestamp when the payment record was created.</summary>
    public DateTime CreatedAt { get; set; }
}
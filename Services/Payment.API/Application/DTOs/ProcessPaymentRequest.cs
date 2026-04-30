namespace Payment.API.Application.DTOs;

/// <summary>
/// Request DTO for manually processing a payment via the API.
/// </summary>
public class ProcessPaymentRequest
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
}

// 'namespace' — logical grouping that prevents name collisions; mirrors the folder structure
namespace Payment.API.Application.DTOs;

/// <summary>
/// Request DTO for manually processing a payment via the API.
/// </summary>
// 'public' — access modifier: visible to the ASP.NET Core model binder and Swagger
// 'class' — reference type; the model binder creates a new instance per HTTP request
public class ProcessPaymentRequest
{
    // 'public' — property must be public for JSON deserialization by System.Text.Json / Newtonsoft.Json
    // 'Guid' — 128-bit globally unique identifier; links this payment request to a specific order
    public Guid OrderId { get; set; }

    // 'public' — accessible to the model binder
    // 'decimal' — 128-bit fixed-point numeric type; used for monetary values to avoid floating-point rounding errors
    public decimal Amount { get; set; }

    // 'public' — accessible to the model binder
    // 'string' — built-in Unicode string type; carries the payment method name (e.g., "Card", "CashOnDelivery")
    // = string.Empty — default initialiser; ensures the property is never null (non-nullable reference type)
    public string PaymentMethod { get; set; } = string.Empty;

    // 'public' — accessible to the model binder
    // 'string?' — nullable reference type (the '?' suffix); the email is optional in this DTO
    // 'null' — the absence of an object reference; valid here because the email may not always be provided
    public string? CustomerEmail { get; set; }
}

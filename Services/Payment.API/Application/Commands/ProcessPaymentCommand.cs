// 'using' — imports the DTOs namespace so ProcessPaymentRequest is available (referenced in XML docs)
using Payment.API.Application.DTOs;

// 'namespace' — logical grouping that prevents name collisions across the solution
namespace Payment.API.Application.Commands;

/// <summary>
/// Command to process a payment for an order.
/// Creates a payment record and publishes a <c>PaymentConfirmedEvent</c> on success.
/// </summary>
/// <param name="OrderId">The unique identifier of the order being paid for.</param>
/// <param name="CustomerId">The unique identifier of the customer making the payment.</param>
/// <param name="Amount">The total amount to charge.</param>
/// <param name="PaymentMethod">The payment method used (e.g., "Card", "CashOnDelivery").</param>
// 'public' — access modifier: visible to MediatR's handler discovery and the DI container
// 'record' — a special class type (C# 9+) that is immutable by default and provides value-based equality;
//            ideal for commands and events because they should not be mutated after creation
// Positional parameters — the compiler auto-generates a constructor, init-only properties,
//                         Deconstruct(), Equals(), GetHashCode(), and ToString() from these parameters
public record ProcessPaymentCommand(
    // 'Guid' — 128-bit globally unique identifier; used as the order's primary key
    Guid OrderId,
    // 'Guid' — 128-bit globally unique identifier; used as the customer's primary key
    Guid CustomerId,
    // 'string' — built-in Unicode string type; carries the customer's email for notifications
    string CustomerEmail,
    // 'decimal' — 128-bit fixed-point numeric type; used for monetary amounts to avoid floating-point errors
    decimal Amount,
    // 'string' — carries the payment method name (e.g., "UpiNow", "CashOnDelivery", "Card")
    string PaymentMethod);

using Payment.API.Application.DTOs;

namespace Payment.API.Application.Commands;

/// <summary>
/// Command to process a payment for an order.
/// Creates a payment record and publishes a <c>PaymentConfirmedEvent</c> on success.
/// </summary>
/// <param name="OrderId">The unique identifier of the order being paid for.</param>
/// <param name="CustomerId">The unique identifier of the customer making the payment.</param>
/// <param name="Amount">The total amount to charge.</param>
/// <param name="PaymentMethod">The payment method used (e.g., "Card", "CashOnDelivery").</param>
public record ProcessPaymentCommand(
    Guid OrderId,
    Guid CustomerId,
    decimal Amount,
    string PaymentMethod);

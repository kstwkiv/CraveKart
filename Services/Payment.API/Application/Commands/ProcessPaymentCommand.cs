using Payment.API.Application.DTOs;

namespace Payment.API.Application.Commands;

public record ProcessPaymentCommand(
    Guid OrderId,
    Guid CustomerId,
    decimal Amount,
    string PaymentMethod);

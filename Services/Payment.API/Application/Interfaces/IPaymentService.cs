using Payment.API.Application.Commands;
using Payment.API.Application.DTOs;

namespace Payment.API.Application.Interfaces;

/// <summary>
/// Service interface for processing payment transactions.
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Processes a payment for an order, creates a payment record, and publishes a confirmation event.
    /// </summary>
    /// <param name="request">The command containing order, customer, and payment details.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PaymentDto"/> representing the processed payment.</returns>
    Task<PaymentDto> ProcessAsync(ProcessPaymentCommand request, CancellationToken cancellationToken = default);
}

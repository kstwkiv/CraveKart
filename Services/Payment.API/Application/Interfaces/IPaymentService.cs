using Payment.API.Application.Commands;
using Payment.API.Application.DTOs;

namespace Payment.API.Application.Interfaces;

/// <summary>
/// Service interface defining the core payment processing operations for the Payment bounded context.
/// </summary>
public interface IPaymentService
{
    /// <summary>Processes a UPI payment immediately — creates a Confirmed record and publishes <c>PaymentConfirmedEvent</c>.</summary>
    /// <param name="request">The command containing order, customer, and amount details.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The created <see cref="PaymentDto"/>.</returns>
    Task<PaymentDto> ProcessAsync(ProcessPaymentCommand request, CancellationToken cancellationToken = default);

    /// <summary>Creates a Pending payment for COD orders — no event published yet.</summary>
    /// <param name="request">The command containing order, customer, and amount details.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The created <see cref="PaymentDto"/> with Pending status.</returns>
    Task<PaymentDto> CreatePendingAsync(ProcessPaymentCommand request, CancellationToken cancellationToken = default);

    /// <summary>Confirms a pending COD payment after delivery — publishes <c>PaymentConfirmedEvent</c>.</summary>
    /// <param name="orderId">The unique identifier of the order whose payment to confirm.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The updated <see cref="PaymentDto"/>, or <c>null</c> if not found.</returns>
    Task<PaymentDto?> ConfirmCodAsync(Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>Retrieves the payment record for a specific order.</summary>
    /// <param name="orderId">The unique identifier of the order.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The <see cref="PaymentDto"/> if found; otherwise <c>null</c>.</returns>
    Task<PaymentDto?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>Retrieves all payment records for a specific customer.</summary>
    /// <param name="customerId">The unique identifier of the customer.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of <see cref="PaymentDto"/> records for the customer.</returns>
    Task<List<PaymentDto>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);

    /// <summary>Retrieves all payment records (admin use).</summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of all <see cref="PaymentDto"/> records.</returns>
    Task<List<PaymentDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Refunds a confirmed payment for a cancelled order and publishes <c>PaymentRefundedEvent</c>.</summary>
    /// <param name="orderId">The unique identifier of the order to refund.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The updated <see cref="PaymentDto"/>, or <c>null</c> if no payment record exists.</returns>
    Task<PaymentDto?> RefundAsync(Guid orderId, CancellationToken cancellationToken = default);
}

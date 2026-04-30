using PaymentEntity = Payment.API.Domain.Entities.Payment;

namespace Payment.API.Application.Interfaces;

/// <summary>
/// Repository interface for managing <see cref="PaymentEntity"/> persistence operations.
/// </summary>
public interface IPaymentRepository
{
    /// <summary>Retrieves a payment record by the associated order ID.</summary>
    /// <param name="orderId">The unique identifier of the order.</param>
    /// <returns>The matching <see cref="PaymentEntity"/>, or <c>null</c> if not found.</returns>
    Task<PaymentEntity?> GetByOrderIdAsync(Guid orderId);

    /// <summary>Adds a new payment record to the repository.</summary>
    /// <param name="payment">The <see cref="PaymentEntity"/> to add.</param>
    Task AddAsync(PaymentEntity payment);

    /// <summary>Marks an existing payment record as modified.</summary>
    /// <param name="payment">The <see cref="PaymentEntity"/> with updated values.</param>
    void Update(PaymentEntity payment);
}
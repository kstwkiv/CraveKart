using PaymentEntity = Payment.API.Domain.Entities.Payment;

namespace Payment.API.Application.Interfaces;

/// <summary>
/// Repository interface for managing <see cref="PaymentEntity"/> persistence operations.
/// </summary>
public interface IPaymentRepository
{
    /// <summary>Retrieves a payment record by the associated order ID.</summary>
    Task<PaymentEntity?> GetByOrderIdAsync(Guid orderId);

    /// <summary>Retrieves all payment records for a specific customer.</summary>
    Task<List<PaymentEntity>> GetByCustomerIdAsync(Guid customerId);

    /// <summary>Retrieves all payment records (admin use).</summary>
    Task<List<PaymentEntity>> GetAllAsync();

    /// <summary>Adds a new payment record to the repository.</summary>
    Task AddAsync(PaymentEntity payment);

    /// <summary>Marks an existing payment record as modified.</summary>
    void Update(PaymentEntity payment);
}
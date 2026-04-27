using PaymentEntity = Payment.API.Domain.Entities.Payment;

namespace Payment.API.Application.Interfaces;

public interface IPaymentRepository
{
    Task<PaymentEntity?> GetByOrderIdAsync(Guid orderId);
    Task AddAsync(PaymentEntity payment);
    void Update(PaymentEntity payment);
}
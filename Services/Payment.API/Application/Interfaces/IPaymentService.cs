using Payment.API.Application.Commands;
using Payment.API.Application.DTOs;

namespace Payment.API.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentDto> ProcessAsync(ProcessPaymentCommand request, CancellationToken cancellationToken = default);
    Task<PaymentDto?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<List<PaymentDto>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<List<PaymentDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PaymentDto?> RefundAsync(Guid orderId, CancellationToken cancellationToken = default);
}

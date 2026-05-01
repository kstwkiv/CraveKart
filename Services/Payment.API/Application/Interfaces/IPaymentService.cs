using Payment.API.Application.Commands;
using Payment.API.Application.DTOs;

namespace Payment.API.Application.Interfaces;

public interface IPaymentService
{
    /// <summary>Processes a UPI payment immediately — creates Confirmed record and publishes PaymentConfirmedEvent.</summary>
    Task<PaymentDto> ProcessAsync(ProcessPaymentCommand request, CancellationToken cancellationToken = default);

    /// <summary>Creates a Pending payment for COD orders — no event published yet.</summary>
    Task<PaymentDto> CreatePendingAsync(ProcessPaymentCommand request, CancellationToken cancellationToken = default);

    /// <summary>Confirms a pending COD payment after delivery — publishes PaymentConfirmedEvent.</summary>
    Task<PaymentDto?> ConfirmCodAsync(Guid orderId, CancellationToken cancellationToken = default);

    Task<PaymentDto?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<List<PaymentDto>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<List<PaymentDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PaymentDto?> RefundAsync(Guid orderId, CancellationToken cancellationToken = default);
}

using Payment.API.Application.Commands;
using Payment.API.Application.DTOs;

namespace Payment.API.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentDto> ProcessAsync(ProcessPaymentCommand request, CancellationToken cancellationToken = default);
}

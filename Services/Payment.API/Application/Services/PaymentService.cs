using FoodFleet.Shared.Events.Payments;
using FoodFleet.Shared.Messaging.Interfaces;
using Payment.API.Application.Commands;
using Payment.API.Application.DTOs;
using Payment.API.Application.Interfaces;
using Payment.API.Domain.Enums;

namespace Payment.API.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;

    public PaymentService(IUnitOfWork unitOfWork, IEventPublisher eventPublisher)
    {
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
    }

    public async Task<PaymentDto> ProcessAsync(ProcessPaymentCommand request, CancellationToken cancellationToken = default)
    {
        var payment = new Domain.Entities.Payment
        {
            OrderId = request.OrderId,
            CustomerId = request.CustomerId,
            CustomerEmail = request.CustomerEmail,
            Amount = request.Amount,
            PaymentMethod = request.PaymentMethod,
            Status = PaymentStatus.Confirmed,
            ProcessedAt = IstClock.Now
        };

        await _unitOfWork.Payments.AddAsync(payment);
        await _unitOfWork.SaveChangesAsync();

        await _eventPublisher.PublishAsync(new PaymentConfirmedEvent
        {
            PaymentId = payment.Id,
            OrderId = payment.OrderId,
            CustomerId = payment.CustomerId,
            CustomerEmail = payment.CustomerEmail,
            Amount = payment.Amount,
            PaymentMethod = payment.PaymentMethod,
            ConfirmedAt = IstClock.Now
        }, cancellationToken);

        return ToDto(payment);
    }

    public async Task<PaymentDto> CreatePendingAsync(ProcessPaymentCommand request, CancellationToken cancellationToken = default)
    {
        var payment = new Domain.Entities.Payment
        {
            OrderId = request.OrderId,
            CustomerId = request.CustomerId,
            CustomerEmail = request.CustomerEmail,
            Amount = request.Amount,
            PaymentMethod = request.PaymentMethod,
            Status = PaymentStatus.Pending,
            ProcessedAt = IstClock.Now
        };

        await _unitOfWork.Payments.AddAsync(payment);
        await _unitOfWork.SaveChangesAsync();

        return ToDto(payment);
    }

    public async Task<PaymentDto?> ConfirmCodAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var payment = await _unitOfWork.Payments.GetByOrderIdAsync(orderId);
        if (payment == null) return null;
        if (payment.Status != PaymentStatus.Pending) return ToDto(payment);

        payment.Status = PaymentStatus.Confirmed;
        payment.ProcessedAt = IstClock.Now;
        _unitOfWork.Payments.Update(payment);
        await _unitOfWork.SaveChangesAsync();

        await _eventPublisher.PublishAsync(new PaymentConfirmedEvent
        {
            PaymentId = payment.Id,
            OrderId = payment.OrderId,
            CustomerId = payment.CustomerId,
            CustomerEmail = payment.CustomerEmail,
            Amount = payment.Amount,
            PaymentMethod = payment.PaymentMethod,
            ConfirmedAt = IstClock.Now
        }, cancellationToken);

        return ToDto(payment);
    }

    public async Task<PaymentDto?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var payment = await _unitOfWork.Payments.GetByOrderIdAsync(orderId);
        return payment == null ? null : ToDto(payment);
    }

    public async Task<List<PaymentDto>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var payments = await _unitOfWork.Payments.GetByCustomerIdAsync(customerId);
        return payments.Select(ToDto).ToList();
    }

    public async Task<List<PaymentDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var payments = await _unitOfWork.Payments.GetAllAsync();
        return payments.Select(ToDto).ToList();
    }

    public async Task<PaymentDto?> RefundAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var payment = await _unitOfWork.Payments.GetByOrderIdAsync(orderId);

        if (payment == null)
        {
            // Payment record missing (e.g. order placed before payment service was running).
            // Return null so the controller can return a clear 404.
            return null;
        }

        if (payment.Status == PaymentStatus.Refunded)
            throw new InvalidOperationException("This payment has already been refunded.");

        if (payment.Status == PaymentStatus.Failed)
            throw new InvalidOperationException("Cannot refund a failed payment.");

        payment.Status = PaymentStatus.Refunded;
        payment.ProcessedAt = IstClock.Now;
        _unitOfWork.Payments.Update(payment);
        await _unitOfWork.SaveChangesAsync();

        await _eventPublisher.PublishAsync(new PaymentRefundedEvent
        {
            PaymentId = payment.Id,
            OrderId = payment.OrderId,
            CustomerId = payment.CustomerId,
            CustomerEmail = payment.CustomerEmail,
            Amount = payment.Amount,
            RefundedAt = IstClock.Now
        }, cancellationToken);

        return ToDto(payment);
    }

    private static PaymentDto ToDto(Domain.Entities.Payment p) => new()
    {
        Id = p.Id,
        OrderId = p.OrderId,
        CustomerId = p.CustomerId,
        Amount = p.Amount,
        Currency = p.Currency,
        Status = p.Status.ToString(),
        PaymentMethod = p.PaymentMethod,
        CreatedAt = p.CreatedAt,
        ProcessedAt = p.ProcessedAt
    };
}

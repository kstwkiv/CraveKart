using FoodFleet.Shared.Events.Payments;
using FoodFleet.Shared.Messaging.Interfaces;
using MediatR;
using Payment.API.Application.Commands;
using Payment.API.Application.DTOs;
using Payment.API.Application.Interfaces;
using Payment.API.Domain.Enums;

namespace Payment.API.Application.Handlers;

public class ProcessPaymentHandler : IRequestHandler<ProcessPaymentCommand, PaymentDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;

    public ProcessPaymentHandler(IUnitOfWork unitOfWork, IEventPublisher eventPublisher)
    {
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
    }

    public async Task<PaymentDto> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = new Domain.Entities.Payment
        {
            OrderId = request.OrderId,
            CustomerId = request.CustomerId,
            Amount = request.Amount,
            PaymentMethod = request.PaymentMethod,
            Status = PaymentStatus.Confirmed,
            ProcessedAt = DateTime.UtcNow
        };

        await _unitOfWork.Payments.AddAsync(payment);
        await _unitOfWork.SaveChangesAsync();

        await _eventPublisher.PublishAsync(new PaymentConfirmedEvent
        {
            PaymentId = payment.Id,
            OrderId = payment.OrderId,
            CustomerId = payment.CustomerId,
            Amount = payment.Amount,
            ConfirmedAt = DateTime.UtcNow
        }, cancellationToken);

        return new PaymentDto
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            Amount = payment.Amount,
            Status = payment.Status.ToString(),
            PaymentMethod = payment.PaymentMethod,
            CreatedAt = payment.CreatedAt
        };
    }
}
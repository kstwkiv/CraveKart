using FoodFleet.Shared.Events.Orders;
using FoodFleet.Shared.Messaging.Interfaces;
using MediatR;
using Order.API.Application.Commands;
using Order.API.Application.Interfaces;

namespace Order.API.Application.Handlers;

public class UpdateOrderStatusHandler : IRequestHandler<UpdateOrderStatusCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;

    public UpdateOrderStatusHandler(IUnitOfWork unitOfWork, IEventPublisher eventPublisher)
    {
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
    }

    public async Task<bool> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId);
        if (order == null) return false;

        var oldStatus = order.Status.ToString();
        order.Status = request.NewStatus;
        order.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync();

        await _eventPublisher.PublishAsync(new OrderStatusChangedEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            CustomerEmail = order.CustomerEmail,
            OldStatus = oldStatus,
            NewStatus = request.NewStatus.ToString(),
            ChangedAt = DateTime.UtcNow
        }, cancellationToken);

        return true;
    }
}
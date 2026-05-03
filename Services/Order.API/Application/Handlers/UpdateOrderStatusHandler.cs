using FoodFleet.Shared.Events.Orders;
using FoodFleet.Shared.Messaging.Interfaces;
using MediatR;
using Order.API.Application.Commands;
using Order.API.Application.Interfaces;

namespace Order.API.Application.Handlers;

/// <summary>
/// MediatR handler that processes <see cref="UpdateOrderStatusCommand"/> requests.
/// Updates the order status and publishes an <see cref="OrderStatusChangedEvent"/> to notify downstream services.
/// </summary>
public class UpdateOrderStatusHandler : IRequestHandler<UpdateOrderStatusCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;

    /// <summary>
    /// Initializes a new instance of <see cref="UpdateOrderStatusHandler"/>.
    /// </summary>
    /// <param name="unitOfWork">The unit of work for data access.</param>
    /// <param name="eventPublisher">The event publisher for raising domain events.</param>
    public UpdateOrderStatusHandler(IUnitOfWork unitOfWork, IEventPublisher eventPublisher)
    {
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
    }

    /// <summary>
    /// Handles the status update request by persisting the new status and publishing the changed event.
    /// </summary>
    /// <param name="request">The command containing the order ID and new status.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns><c>true</c> if the status was updated; <c>false</c> if the order was not found.</returns>
    public async Task<bool> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId);
        if (order == null) return false;

        var oldStatus = order.Status.ToString();
        order.Status = request.NewStatus;
        order.UpdatedAt = IstClock.Now;
        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync();

        await _eventPublisher.PublishAsync(new OrderStatusChangedEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            CustomerEmail = order.CustomerEmail,
            OldStatus = oldStatus,
            NewStatus = request.NewStatus.ToString(),
            ChangedAt = IstClock.Now
        }, cancellationToken);

        return true;
    }
}
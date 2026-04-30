using Delivery.API.Application.Commands;
using Delivery.API.Application.Interfaces;
using FoodFleet.Shared.Events.Delivery;
using FoodFleet.Shared.Messaging.Interfaces;
using MediatR;

namespace Delivery.API.Application.Handlers;

/// <summary>
/// MediatR handler that processes <see cref="CompleteDeliveryCommand"/> requests.
/// Marks the delivery as completed, updates agent statistics, and publishes a <see cref="DeliveryCompletedEvent"/>.
/// </summary>
public class CompleteDeliveryHandler : IRequestHandler<CompleteDeliveryCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;

    /// <summary>
    /// Initializes a new instance of <see cref="CompleteDeliveryHandler"/>.
    /// </summary>
    /// <param name="unitOfWork">The unit of work for data access.</param>
    /// <param name="eventPublisher">The event publisher for raising domain events.</param>
    public CompleteDeliveryHandler(IUnitOfWork unitOfWork, IEventPublisher eventPublisher)
    {
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
    }

    /// <summary>
    /// Handles the completion of a delivery, updating status, agent stats, and publishing the completion event.
    /// </summary>
    /// <param name="request">The command containing the order ID to complete.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns><c>true</c> if the delivery was completed; <c>false</c> if the delivery was not found.</returns>
    public async Task<bool> Handle(CompleteDeliveryCommand request, CancellationToken cancellationToken)
    {
        var delivery = await _unitOfWork.Deliveries.GetByOrderIdAsync(request.OrderId);
        if (delivery == null) return false;

        delivery.Status = "Delivered";
        delivery.CompletedAt = IstClock.Now;
        _unitOfWork.Deliveries.Update(delivery);

        // Update agent stats
        var agent = delivery.Agent;
        if (agent != null)
        {
            agent.IsAvailable = true;
            agent.TotalDeliveries++;
            _unitOfWork.Deliveries.UpdateAgent(agent);
        }

        await _unitOfWork.SaveChangesAsync();

        await _eventPublisher.PublishAsync(new DeliveryCompletedEvent
        {
            OrderId = delivery.OrderId,
            AgentId = delivery.AgentId,
            CustomerId = delivery.CustomerId,
            CustomerEmail = delivery.CustomerEmail,
            CompletedAt = delivery.CompletedAt!.Value
        }, cancellationToken);

        return true;
    }
}

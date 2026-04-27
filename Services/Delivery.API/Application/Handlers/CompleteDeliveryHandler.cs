using Delivery.API.Application.Commands;
using Delivery.API.Application.Interfaces;
using FoodFleet.Shared.Events.Delivery;
using FoodFleet.Shared.Messaging.Interfaces;
using MediatR;

namespace Delivery.API.Application.Handlers;

public class CompleteDeliveryHandler : IRequestHandler<CompleteDeliveryCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;

    public CompleteDeliveryHandler(IUnitOfWork unitOfWork, IEventPublisher eventPublisher)
    {
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
    }

    public async Task<bool> Handle(CompleteDeliveryCommand request, CancellationToken cancellationToken)
    {
        var delivery = await _unitOfWork.Deliveries.GetByOrderIdAsync(request.OrderId);
        if (delivery == null) return false;

        delivery.Status = "Delivered";
        delivery.CompletedAt = DateTime.UtcNow;
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

using Delivery.API.Application.Commands;
using Delivery.API.Application.DTOs;
using Delivery.API.Application.Interfaces;
using Delivery.API.Domain.Entities;
using FoodFleet.Shared.Events.Delivery;
using FoodFleet.Shared.Messaging.Interfaces;
using MediatR;

namespace Delivery.API.Application.Handlers;

public class AssignDeliveryHandler : IRequestHandler<AssignDeliveryCommand, DeliveryDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;

    public AssignDeliveryHandler(IUnitOfWork unitOfWork, IEventPublisher eventPublisher)
    {
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
    }

    public async Task<DeliveryDto> Handle(AssignDeliveryCommand request, CancellationToken cancellationToken)
    {
        var agent = await _unitOfWork.Deliveries.GetAvailableAgentAsync()
            ?? throw new Exception("No available delivery agents.");

        agent.IsAvailable = false;
        _unitOfWork.Deliveries.UpdateAgent(agent);

        var delivery = new Domain.Entities.Delivery
        {
            OrderId = request.OrderId,
            AgentId = agent.Id,
            CustomerId = request.CustomerId,
            CustomerEmail = request.CustomerEmail,
            Status = "Assigned",
            AssignedAt = DateTime.UtcNow
        };

        await _unitOfWork.Deliveries.AddAsync(delivery);
        await _unitOfWork.SaveChangesAsync();

        await _eventPublisher.PublishAsync(new DeliveryAssignedEvent
        {
            OrderId = request.OrderId,
            AgentId = agent.Id,
            AgentName = agent.FullName,
            CustomerId = request.CustomerId,
            CustomerEmail = request.CustomerEmail,
            AssignedAt = DateTime.UtcNow
        }, cancellationToken);

        return new DeliveryDto
        {
            Id = delivery.Id,
            OrderId = delivery.OrderId,
            AgentId = delivery.AgentId,
            Status = delivery.Status,
            AssignedAt = delivery.AssignedAt
        };
    }
}
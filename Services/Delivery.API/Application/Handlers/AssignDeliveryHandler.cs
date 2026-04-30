using Delivery.API.Application.Commands;
using Delivery.API.Application.DTOs;
using Delivery.API.Application.Interfaces;
using Delivery.API.Domain.Entities;
using FoodFleet.Shared.Events.Delivery;
using FoodFleet.Shared.Messaging.Interfaces;
using MediatR;

namespace Delivery.API.Application.Handlers;

/// <summary>
/// MediatR handler that processes <see cref="AssignDeliveryCommand"/> requests.
/// Finds an available agent, creates a delivery record, and publishes a <see cref="DeliveryAssignedEvent"/>.
/// </summary>
public class AssignDeliveryHandler : IRequestHandler<AssignDeliveryCommand, DeliveryDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;

    /// <summary>
    /// Initializes a new instance of <see cref="AssignDeliveryHandler"/>.
    /// </summary>
    /// <param name="unitOfWork">The unit of work for data access.</param>
    /// <param name="eventPublisher">The event publisher for raising domain events.</param>
    public AssignDeliveryHandler(IUnitOfWork unitOfWork, IEventPublisher eventPublisher)
    {
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
    }

    /// <summary>
    /// Handles the assignment of an available delivery agent to the specified order.
    /// </summary>
    /// <param name="request">The command containing order and customer details.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="DeliveryDto"/> representing the created delivery assignment.</returns>
    /// <exception cref="Exception">Thrown when no available delivery agents are found.</exception>
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
            AssignedAt = IstClock.Now
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
            AssignedAt = IstClock.Now
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
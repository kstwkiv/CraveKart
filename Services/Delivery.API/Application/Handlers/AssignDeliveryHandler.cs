// 'using' — imports the Commands namespace so AssignDeliveryCommand is in scope
using Delivery.API.Application.Commands;
// 'using' — imports the DTOs namespace so DeliveryDto is available as a return type
using Delivery.API.Application.DTOs;
// 'using' — imports the Interfaces namespace containing IUnitOfWork
using Delivery.API.Application.Interfaces;
// 'using' — imports the Delivery domain entities namespace
using Delivery.API.Domain.Entities;
// 'using' — imports the shared Delivery events namespace (DeliveryAssignedEvent)
using FoodFleet.Shared.Events.Delivery;
// 'using' — imports the shared messaging abstraction (IEventPublisher)
using FoodFleet.Shared.Messaging.Interfaces;
// 'using' — imports MediatR so IRequestHandler<,> is available
using MediatR;

// 'namespace' — declares a logical scope; prevents type-name collisions across the solution
namespace Delivery.API.Application.Handlers;

/// <summary>
/// MediatR handler that processes <see cref="AssignDeliveryCommand"/> requests.
/// Finds an available agent, creates a delivery record, and publishes a <see cref="DeliveryAssignedEvent"/>.
/// </summary>
// 'public' — access modifier: this class is visible outside its assembly (required by MediatR's DI registration)
// 'class' — reference type; each instance is a separate object on the managed heap
// IRequestHandler<AssignDeliveryCommand, DeliveryDto> — MediatR contract:
//   receives AssignDeliveryCommand and produces a DeliveryDto result
public class AssignDeliveryHandler : IRequestHandler<AssignDeliveryCommand, DeliveryDto>
{
    // 'private' — encapsulates the field; only this class can read or write it
    // 'readonly' — the reference is set once in the constructor and never changed (immutability guarantee)
    private readonly IUnitOfWork _unitOfWork;

    // 'private readonly' — same immutability guarantee for the event publisher dependency
    private readonly IEventPublisher _eventPublisher;

    /// <summary>
    /// Initializes a new instance of <see cref="AssignDeliveryHandler"/>.
    /// </summary>
    /// <param name="unitOfWork">The unit of work for data access.</param>
    /// <param name="eventPublisher">The event publisher for raising domain events.</param>
    // 'public' — constructor visibility required for Dependency Injection container instantiation
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
    // 'public' — satisfies the IRequestHandler interface contract
    // 'async' — marks the method as asynchronous; the compiler rewrites it as a state machine
    // 'Task<DeliveryDto>' — Task represents the in-flight async work; DeliveryDto is the eventual result
    public async Task<DeliveryDto> Handle(AssignDeliveryCommand request, CancellationToken cancellationToken)
    {
        // 'var' — implicitly typed; compiler infers DeliveryAgent? from GetAvailableAgentAsync
        // 'await' — suspends execution until the database query completes, freeing the thread
        // '??' — null-coalescing operator: if the left side is null, evaluate the right side
        // 'throw' — raises an exception when no agent is available, unwinding the call stack
        var agent = await _unitOfWork.Deliveries.GetAvailableAgentAsync()
            ?? throw new Exception("No available delivery agents.");

        // 'false' — boolean literal; marks the agent as busy so it won't be assigned again
        agent.IsAvailable = false;
        _unitOfWork.Deliveries.UpdateAgent(agent); // marks the agent entity as modified in the change tracker

        // 'var' — compiler infers Domain.Entities.Delivery
        // 'new' — allocates a new Delivery entity object on the managed heap
        var delivery = new Domain.Entities.Delivery
        {
            OrderId = request.OrderId,
            AgentId = agent.Id,
            CustomerId = request.CustomerId,
            CustomerEmail = request.CustomerEmail,
            Status = "Assigned",       // initial status string for the delivery record
            AssignedAt = IstClock.Now  // IST timestamp of assignment
        };

        // 'await' — asynchronously persists the new delivery record
        await _unitOfWork.Deliveries.AddAsync(delivery);
        // 'await' — flushes all pending changes to the database atomically
        await _unitOfWork.SaveChangesAsync();

        // 'await' — asynchronously publishes the domain event to the message bus
        // 'new' — allocates a new DeliveryAssignedEvent object
        await _eventPublisher.PublishAsync(new DeliveryAssignedEvent
        {
            OrderId = request.OrderId,
            AgentId = agent.Id,
            AgentName = agent.FullName,
            CustomerId = request.CustomerId,
            CustomerEmail = request.CustomerEmail,
            AssignedAt = IstClock.Now
        }, cancellationToken); // pass the original token so callers can cancel the publish

        // 'return' — exits the method and hands the DeliveryDto back to the MediatR pipeline
        // 'new' — allocates the DTO that carries the result back to the controller
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

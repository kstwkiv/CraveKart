// 'using' — imports the Commands namespace so CompleteDeliveryCommand is in scope
using Delivery.API.Application.Commands;
// 'using' — imports the Interfaces namespace containing IUnitOfWork
using Delivery.API.Application.Interfaces;
// 'using' — imports the shared Delivery domain events namespace
using FoodFleet.Shared.Events.Delivery;
// 'using' — imports the shared messaging abstraction (IEventPublisher)
using FoodFleet.Shared.Messaging.Interfaces;
// 'using' — imports MediatR so IRequestHandler<,> is available
using MediatR;

// 'namespace' — logical grouping that prevents name collisions across assemblies
namespace Delivery.API.Application.Handlers;

/// <summary>
/// MediatR handler that processes <see cref="CompleteDeliveryCommand"/> requests.
/// Marks the delivery as completed, updates agent statistics, and publishes a <see cref="DeliveryCompletedEvent"/>.
/// </summary>
// 'public' — access modifier: visible to any code that references this assembly
// 'class' — defines a reference type; instances live on the managed heap
// IRequestHandler<CompleteDeliveryCommand, bool> — MediatR interface contract:
//   the handler receives a CompleteDeliveryCommand and returns a bool result
public class CompleteDeliveryHandler : IRequestHandler<CompleteDeliveryCommand, bool>
{
    // 'private readonly' — field is encapsulated within this class and cannot be reassigned after construction
    private readonly IUnitOfWork _unitOfWork;

    // 'private readonly' — IEventPublisher is an abstraction; the concrete bus is injected at runtime
    private readonly IEventPublisher _eventPublisher;

    /// <summary>
    /// Initializes a new instance of <see cref="CompleteDeliveryHandler"/>.
    /// </summary>
    /// <param name="unitOfWork">The unit of work for data access.</param>
    /// <param name="eventPublisher">The event publisher for raising domain events.</param>
    // 'public' — constructor must be public so the DI container can create instances
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
    // 'public' — required by the IRequestHandler interface contract
    // 'async' — enables the use of 'await' inside; the compiler generates a state machine
    // 'Task<bool>' — Task wraps the asynchronous operation; bool is the eventual result
    // 'override' is not needed here because IRequestHandler uses explicit interface implementation
    public async Task<bool> Handle(CompleteDeliveryCommand request, CancellationToken cancellationToken)
    {
        // 'var' — implicitly typed; compiler infers the type from GetByOrderIdAsync's return type
        // 'await' — suspends the method until the database query resolves, freeing the thread
        var delivery = await _unitOfWork.Deliveries.GetByOrderIdAsync(request.OrderId);
        // 'if' — conditional: if no delivery was found, exit early
        // 'null' — represents the absence of an object; returned when no record matches
        if (delivery == null) return false; // 'return false' — exits the method with a "not found" signal

        delivery.Status = "Delivered"; // update the status string on the entity
        delivery.CompletedAt = IstClock.Now; // record the IST completion timestamp
        _unitOfWork.Deliveries.Update(delivery); // marks the entity as modified in the EF change tracker

        // Update agent stats
        // 'var' — compiler infers DeliveryAgent? (nullable reference type)
        var agent = delivery.Agent;
        // 'if' — null-guard: only update agent stats if the navigation property was loaded
        if (agent != null)
        {
            // 'true' — boolean literal; marks the agent as available for new deliveries
            agent.IsAvailable = true;
            agent.TotalDeliveries++; // '++' — post-increment operator; adds 1 to the integer field
            agent.TotalEarnings += 100m; // ₹100 per completed delivery — must match EARN_PER_DELIVERY on the frontend
            _unitOfWork.Deliveries.UpdateAgent(agent);
        }

        // 'await' — asynchronously flushes all tracked changes to the database in one transaction
        await _unitOfWork.SaveChangesAsync();

        // 'await' — asynchronously publishes the domain event to the message bus
        // 'new' — allocates a new DeliveryCompletedEvent object on the heap
        await _eventPublisher.PublishAsync(new DeliveryCompletedEvent
        {
            OrderId = delivery.OrderId,
            AgentId = delivery.AgentId,
            CustomerId = delivery.CustomerId,
            CustomerEmail = delivery.CustomerEmail,
            CompletedAt = delivery.CompletedAt!.Value // '!' — null-forgiving operator: asserts the value is not null here
        }, cancellationToken); // pass the original token so callers can cancel the publish

        // 'return true' — signals to the caller that the delivery was successfully completed
        return true;
    }
}

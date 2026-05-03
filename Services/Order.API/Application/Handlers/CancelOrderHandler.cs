// 'using' — imports the shared Orders events namespace so OrderCancelledEvent is in scope
using FoodFleet.Shared.Events.Orders;
// 'using' — imports the shared messaging abstraction (IEventPublisher)
using FoodFleet.Shared.Messaging.Interfaces;
// 'using' — imports MediatR so IRequestHandler<,> is available
using MediatR;
// 'using' — imports the Order application commands namespace (CancelOrderCommand)
using Order.API.Application.Commands;
// 'using' — imports the Order application interfaces namespace (IUnitOfWork)
using Order.API.Application.Interfaces;
// 'using' — imports the OrderStatus enum from the domain layer
using Order.API.Domain.Enums;

// 'namespace' — logical grouping that prevents name collisions across the solution
namespace Order.API.Application.Handlers;

/// <summary>
/// MediatR handler that processes <see cref="CancelOrderCommand"/> requests.
/// Validates the order is in a cancellable state, updates its status, and publishes
/// an <see cref="OrderCancelledEvent"/> to trigger a payment refund.
/// </summary>
// 'public' — access modifier: visible to the DI container and MediatR's handler discovery
// 'class' — reference type; each MediatR dispatch creates a fresh instance (scoped lifetime)
// IRequestHandler<CancelOrderCommand, bool> — MediatR contract:
//   receives CancelOrderCommand and returns a bool indicating success
public class CancelOrderHandler : IRequestHandler<CancelOrderCommand, bool>
{
    // 'private readonly' — encapsulated, immutable after construction; set via Dependency Injection
    private readonly IUnitOfWork _unitOfWork;

    // 'private readonly' — IEventPublisher is an abstraction; the concrete bus is injected at runtime
    private readonly IEventPublisher _eventPublisher;

    /// <summary>
    /// Initializes a new instance of <see cref="CancelOrderHandler"/>.
    /// </summary>
    /// <param name="unitOfWork">The unit of work for data access.</param>
    /// <param name="eventPublisher">The event publisher for raising domain events.</param>
    // 'public' — constructor must be public for the DI container to instantiate this handler
    public CancelOrderHandler(IUnitOfWork unitOfWork, IEventPublisher eventPublisher)
    {
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
    }

    /// <summary>
    /// Handles the cancel order request by updating the order status and publishing the cancellation event.
    /// </summary>
    /// <param name="request">The command containing the order and customer IDs.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns><c>true</c> if the order was cancelled; <c>false</c> if the order was not found.</returns>
    /// <exception cref="Exception">Thrown when the order is not in a cancellable state.</exception>
    // 'public' — satisfies the IRequestHandler interface contract
    // 'async' — enables await; the compiler generates a state machine for non-blocking execution
    // 'Task<bool>' — Task wraps the async operation; bool is the eventual result value
    public async Task<bool> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        // 'var' — implicitly typed; compiler infers Order? (nullable) from GetByIdAsync's return type
        // 'await' — suspends execution until the database query resolves, freeing the thread
        var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId);
        // 'if' — conditional: if no order was found, return false (not-found signal)
        // 'null' — represents the absence of an object reference
        if (order == null) return false; // 'return false' — exits early without throwing

        // 'if' — business-rule guard: only Placed or Confirmed orders can be cancelled
        // '!=' — inequality operator; checks that the status is neither Placed nor Confirmed
        if (order.Status != OrderStatus.Placed && order.Status != OrderStatus.Confirmed)
            // 'throw' — raises an exception, unwinding the call stack to the nearest catch block
            throw new Exception("Order cannot be cancelled at this stage.");

        // Assign the Cancelled enum member to the Status property
        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = IstClock.Now; // record the IST timestamp of the cancellation
        _unitOfWork.Orders.Update(order); // marks the entity as modified in the EF change tracker
        // 'await' — asynchronously flushes all tracked changes to the database in one transaction
        await _unitOfWork.SaveChangesAsync();

        // 'await' — asynchronously publishes the domain event to the message bus
        // 'new' — allocates a new OrderCancelledEvent object on the managed heap
        await _eventPublisher.PublishAsync(new OrderCancelledEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            CustomerEmail = order.CustomerEmail,
            Reason = "Cancelled by customer", // string literal describing the cancellation reason
            CancelledAt = IstClock.Now
        }, cancellationToken); // pass the original token so callers can cancel the publish

        // 'return true' — signals to the caller that the order was successfully cancelled
        return true;
    }
}

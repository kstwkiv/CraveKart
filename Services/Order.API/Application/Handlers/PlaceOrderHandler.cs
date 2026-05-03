// 'using' — imports the shared Orders events namespace so OrderPlacedEvent and OrderPlacedItemEvent are in scope
using FoodFleet.Shared.Events.Orders;
// 'using' — imports the messaging interfaces namespace for IEventPublisher (publish domain events to the bus)
using FoodFleet.Shared.Messaging.Interfaces;
// 'using' — imports MediatR, the library that implements the Mediator pattern (CQRS command dispatching)
using MediatR;
// 'using' — imports the PlaceOrderCommand that carries the data for placing an order
using Order.API.Application.Commands;
// 'using' — imports OrderDto, the data transfer object returned to the caller after the order is placed
using Order.API.Application.DTOs;
// 'using' — imports the IUnitOfWork interface for database operations (Unit of Work pattern)
using Order.API.Application.Interfaces;
// 'using' — imports domain entities (Order, OrderItem) from the Domain layer
using Order.API.Domain.Entities;
// 'using' — imports the OrderStatus enum that represents the lifecycle state of an order
using Order.API.Domain.Enums;

// 'namespace' — logical grouping that scopes this handler to the Order.API Handlers layer
namespace Order.API.Application.Handlers;

/// <summary>
/// MediatR handler that processes <see cref="PlaceOrderCommand"/> requests.
/// Calculates order totals, persists the order, and publishes an <see cref="OrderPlacedEvent"/>
/// to trigger downstream payment processing.
/// </summary>
// 'public' — access modifier: class is visible to all assemblies (required for MediatR registration)
// 'class' — defines a reference type; the blueprint for the handler object
// 'IRequestHandler<TRequest, TResponse>' — MediatR contract; forces implementation of Handle()
public class PlaceOrderHandler : IRequestHandler<PlaceOrderCommand, OrderDto>
{
    // 'private' — encapsulation: field is hidden from outside this class
    // 'readonly' — field is set once in the constructor and never reassigned (immutability)
    private readonly IUnitOfWork _unitOfWork;
    // 'private readonly' — event publisher injected via constructor; immutable after construction
    private readonly IEventPublisher _eventPublisher;

    /// <summary>
    /// Initializes a new instance of <see cref="PlaceOrderHandler"/>.
    /// </summary>
    /// <param name="unitOfWork">The unit of work for data access.</param>
    /// <param name="eventPublisher">The event publisher for raising domain events.</param>
    // 'public' — constructor must be public so the DI container can instantiate this handler
    public PlaceOrderHandler(IUnitOfWork unitOfWork, IEventPublisher eventPublisher)
    {
        // Assigning injected dependencies to readonly fields (Dependency Inversion Principle)
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
    }

    /// <summary>
    /// Handles the place order request by computing totals, saving the order, and publishing the event.
    /// </summary>
    /// <param name="request">The command containing customer, restaurant, and item details.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>An <see cref="OrderDto"/> representing the newly created order.</returns>
    // 'public' — satisfies the IRequestHandler interface; MediatR's dispatcher calls this method
    // 'async' — marks the method as asynchronous; the compiler rewrites it as a state machine
    // 'Task<OrderDto>' — represents a future value; the method will eventually return an OrderDto
    public async Task<OrderDto> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        // 'var' — implicitly typed; compiler infers decimal from the Sum() LINQ expression
        var subTotal = request.Items.Sum(i => i.UnitPrice * i.Quantity);
        // 'm' suffix — decimal literal; 'm' denotes the decimal type (not double) for monetary precision
        var deliveryFee = 30m;
        // 'var' — compiler infers decimal; tax is 5% of the subtotal
        var tax = subTotal * 0.05m;

        // 'var' — compiler infers Domain.Entities.Order from the right-hand side
        // 'new' — allocates a new Order entity on the managed heap using object-initializer syntax
        var order = new Domain.Entities.Order
        {
            CustomerId = request.CustomerId,
            CustomerEmail = request.CustomerEmail,
            RestaurantId = request.RestaurantId,
            DeliveryAddress = request.DeliveryAddress,
            PaymentMethod = request.PaymentMethod,
            // OrderStatus.Placed — enum value representing the initial state in the order lifecycle
            Status = OrderStatus.Placed,
            SubTotal = subTotal,
            DeliveryFee = deliveryFee,
            Tax = tax,
            TotalAmount = subTotal + deliveryFee + tax,
            // LINQ Select projects each command item into a new OrderItem entity
            // 'new' — allocates each OrderItem inside the projection lambda
            OrderItems = request.Items.Select(i => new OrderItem
            {
                MenuItemId = i.MenuItemId,
                MenuItemName = i.MenuItemName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Customizations = i.Customizations
            }).ToList() // ToList() materialises the lazy IEnumerable into a concrete List<OrderItem>
        };

        // 'await' — suspends execution (without blocking the thread) until the async AddAsync completes
        await _unitOfWork.Orders.AddAsync(order);
        // 'await' — asynchronously commits all tracked changes to the database in one transaction
        await _unitOfWork.SaveChangesAsync();

        // 'await' — asynchronously publishes the OrderPlacedEvent to the message bus (e.g. RabbitMQ)
        // 'new' — creates the event payload object that other microservices will consume
        await _eventPublisher.PublishAsync(new OrderPlacedEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            RestaurantId = order.RestaurantId,
            DeliveryAddress = order.DeliveryAddress,
            SubTotal = order.SubTotal,
            DeliveryFee = order.DeliveryFee,
            Tax = order.Tax,
            TotalAmount = order.TotalAmount,
            PaymentMethod = order.PaymentMethod.ToString(),
            CustomerEmail = order.CustomerEmail,
            PlacedAt = IstClock.Now,
            // LINQ Select projects each OrderItem into a lightweight OrderPlacedItemEvent DTO
            Items = order.OrderItems.Select(i => new OrderPlacedItemEvent
            {
                MenuItemName = i.MenuItemName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Customizations = i.Customizations
            }).ToList() // ToList() forces immediate evaluation of the projection
        }, cancellationToken);

        // 'return' — exits the method and resolves the Task<OrderDto> with the constructed DTO
        // 'new' — allocates the OrderDto that is sent back to the API controller / caller
        return new OrderDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            RestaurantId = order.RestaurantId,
            RestaurantName = request.RestaurantName,
            RestaurantLogoUrl = request.RestaurantLogoUrl,
            DeliveryAddress = order.DeliveryAddress,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            PaymentMethod = order.PaymentMethod.ToString(),
            CreatedAt = order.CreatedAt,
            Items = request.Items
        };
    }
}

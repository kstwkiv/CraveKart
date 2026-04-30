using Order.API.Application.Commands;
using Order.API.Application.DTOs;

namespace Order.API.Application.Interfaces;

/// <summary>
/// Service interface defining the core order management operations.
/// </summary>
public interface IOrderService
{
    /// <summary>Places a new order, calculates totals, and publishes an order placed event.</summary>
    /// <param name="request">The command containing order details.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The created <see cref="OrderDto"/>.</returns>
    Task<OrderDto> PlaceOrderAsync(PlaceOrderCommand request, CancellationToken cancellationToken = default);

    /// <summary>Retrieves a single order by its unique identifier.</summary>
    /// <param name="orderId">The unique identifier of the order.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The <see cref="OrderDto"/> if found; otherwise <c>null</c>.</returns>
    Task<OrderDto?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>Retrieves the order history for a specific customer.</summary>
    /// <param name="customerId">The unique identifier of the customer.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of <see cref="OrderDto"/> records for the customer.</returns>
    Task<List<OrderDto>> GetHistoryAsync(Guid customerId, CancellationToken cancellationToken = default);

    /// <summary>Cancels an order if it is in a cancellable state.</summary>
    /// <param name="request">The command containing the order and customer IDs.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns><c>true</c> if cancelled; <c>false</c> if not found.</returns>
    Task<bool> CancelAsync(CancelOrderCommand request, CancellationToken cancellationToken = default);

    /// <summary>Retrieves all orders for a specific restaurant.</summary>
    /// <param name="restaurantId">The unique identifier of the restaurant.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A collection of <see cref="OrderDto"/> records for the restaurant.</returns>
    Task<IEnumerable<OrderDto>> GetByRestaurantAsync(Guid restaurantId, CancellationToken cancellationToken = default);

    /// <summary>Updates the status of an order and publishes a status changed event.</summary>
    /// <param name="request">The command containing the order ID and new status.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns><c>true</c> if updated; <c>false</c> if not found.</returns>
    Task<bool> UpdateStatusAsync(UpdateOrderStatusCommand request, CancellationToken cancellationToken = default);

    /// <summary>Retrieves all orders with a specific status.</summary>
    /// <param name="status">The order status to filter by.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A collection of <see cref="OrderDto"/> records matching the status.</returns>
    Task<IEnumerable<OrderDto>> GetByStatusAsync(Order.API.Domain.Enums.OrderStatus status, CancellationToken cancellationToken = default);
}

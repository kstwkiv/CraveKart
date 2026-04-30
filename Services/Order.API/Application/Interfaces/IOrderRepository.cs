using Order.API.Domain.Enums;
using OrderEntity = Order.API.Domain.Entities.Order;

namespace Order.API.Application.Interfaces;

/// <summary>
/// Repository interface for managing <see cref="OrderEntity"/> persistence operations.
/// </summary>
public interface IOrderRepository
{
    /// <summary>Retrieves an order by its unique identifier, including order items.</summary>
    /// <param name="id">The unique identifier of the order.</param>
    /// <returns>The matching <see cref="OrderEntity"/>, or <c>null</c> if not found.</returns>
    Task<OrderEntity?> GetByIdAsync(Guid id);

    /// <summary>Retrieves all orders for a specific customer, ordered by creation date descending.</summary>
    /// <param name="customerId">The unique identifier of the customer.</param>
    /// <returns>A collection of <see cref="OrderEntity"/> records for the customer.</returns>
    Task<IEnumerable<OrderEntity>> GetByCustomerIdAsync(Guid customerId);

    /// <summary>Retrieves all orders for a specific restaurant, ordered by creation date descending.</summary>
    /// <param name="restaurantId">The unique identifier of the restaurant.</param>
    /// <returns>A collection of <see cref="OrderEntity"/> records for the restaurant.</returns>
    Task<IEnumerable<OrderEntity>> GetByRestaurantIdAsync(Guid restaurantId);

    /// <summary>Retrieves all orders in the system, ordered by creation date descending.</summary>
    /// <returns>A collection of all <see cref="OrderEntity"/> records.</returns>
    Task<IEnumerable<OrderEntity>> GetAllAsync();

    /// <summary>Retrieves all orders with a specific status.</summary>
    /// <param name="status">The order status to filter by.</param>
    /// <returns>A collection of <see cref="OrderEntity"/> records matching the status.</returns>
    Task<IEnumerable<OrderEntity>> GetByStatusAsync(OrderStatus status);

    /// <summary>Adds a new order to the repository.</summary>
    /// <param name="order">The <see cref="OrderEntity"/> to add.</param>
    Task AddAsync(OrderEntity order);

    /// <summary>Marks an existing order as modified.</summary>
    /// <param name="order">The <see cref="OrderEntity"/> with updated values.</param>
    void Update(OrderEntity order);
}
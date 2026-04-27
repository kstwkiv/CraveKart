using Order.API.Domain.Enums;
using OrderEntity = Order.API.Domain.Entities.Order;

namespace Order.API.Application.Interfaces;

public interface IOrderRepository
{
    Task<OrderEntity?> GetByIdAsync(Guid id);
    Task<IEnumerable<OrderEntity>> GetByCustomerIdAsync(Guid customerId);
    Task<IEnumerable<OrderEntity>> GetByRestaurantIdAsync(Guid restaurantId);
    Task<IEnumerable<OrderEntity>> GetAllAsync();
    Task<IEnumerable<OrderEntity>> GetByStatusAsync(OrderStatus status);
    Task AddAsync(OrderEntity order);
    void Update(OrderEntity order);
}
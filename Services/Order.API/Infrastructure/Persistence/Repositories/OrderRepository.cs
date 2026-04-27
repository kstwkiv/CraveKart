using Microsoft.EntityFrameworkCore;
using Order.API.Application.Interfaces;
using OrderEntity = Order.API.Domain.Entities.Order;

namespace Order.API.Infrastructure.Persistence.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _context;

    public OrderRepository(OrderDbContext context)
    {
        _context = context;
    }

    public async Task<OrderEntity?> GetByIdAsync(Guid id) =>
        await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task<IEnumerable<OrderEntity>> GetByCustomerIdAsync(Guid customerId) =>
        await _context.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<OrderEntity>> GetByRestaurantIdAsync(Guid restaurantId) =>
        await _context.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.RestaurantId == restaurantId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<OrderEntity>> GetAllAsync() =>
        await _context.Orders
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<OrderEntity>> GetByStatusAsync(Order.API.Domain.Enums.OrderStatus status) =>
        await _context.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

    public async Task AddAsync(OrderEntity order) =>
        await _context.Orders.AddAsync(order);

    public void Update(OrderEntity order) =>
        _context.Orders.Update(order);
}
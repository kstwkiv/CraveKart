using Microsoft.EntityFrameworkCore;
using Order.API.Application.Interfaces;
using OrderEntity = Order.API.Domain.Entities.Order;

namespace Order.API.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IOrderRepository"/> for managing order records.
/// </summary>
public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="OrderRepository"/>.
    /// </summary>
    /// <param name="context">The database context to use for data access.</param>
    public OrderRepository(OrderDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<OrderEntity?> GetByIdAsync(Guid id) =>
        await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);

    /// <inheritdoc/>
    public async Task<IEnumerable<OrderEntity>> GetByCustomerIdAsync(Guid customerId) =>
        await _context.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

    /// <inheritdoc/>
    public async Task<IEnumerable<OrderEntity>> GetByRestaurantIdAsync(Guid restaurantId) =>
        await _context.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.RestaurantId == restaurantId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

    /// <inheritdoc/>
    public async Task<IEnumerable<OrderEntity>> GetAllAsync() =>
        await _context.Orders
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

    /// <inheritdoc/>
    public async Task<IEnumerable<OrderEntity>> GetByStatusAsync(Order.API.Domain.Enums.OrderStatus status) =>
        await _context.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

    /// <inheritdoc/>
    public async Task AddAsync(OrderEntity order) =>
        await _context.Orders.AddAsync(order);

    /// <inheritdoc/>
    public void Update(OrderEntity order) =>
        _context.Orders.Update(order);
}
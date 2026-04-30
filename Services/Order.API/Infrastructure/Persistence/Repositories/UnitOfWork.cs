using Order.API.Application.Interfaces;

namespace Order.API.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IUnitOfWork"/> for the Order bounded context.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly OrderDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="UnitOfWork"/> and creates the order repository.
    /// </summary>
    /// <param name="context">The shared database context.</param>
    public UnitOfWork(OrderDbContext context)
    {
        _context = context;
        Orders = new OrderRepository(context);
    }

    /// <inheritdoc/>
    public IOrderRepository Orders { get; }

    /// <inheritdoc/>
    public async Task<int> SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}
using Order.API.Application.Interfaces;

namespace Order.API.Infrastructure.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly OrderDbContext _context;

    public UnitOfWork(OrderDbContext context)
    {
        _context = context;
        Orders = new OrderRepository(context);
    }

    public IOrderRepository Orders { get; }

    public async Task<int> SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}
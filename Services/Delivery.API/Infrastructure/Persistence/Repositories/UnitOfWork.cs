using Delivery.API.Application.Interfaces;
using Delivery.API.Infrastructure.Persistence;

namespace Delivery.API.Infrastructure.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly DeliveryDbContext _context;

    public UnitOfWork(DeliveryDbContext context)
    {
        _context = context;
        Deliveries = new DeliveryRepository(context);
        Agents = new AgentRepository(context);
    }

    public IDeliveryRepository Deliveries { get; }
    public IAgentRepository Agents { get; }

    public async Task<int> SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}
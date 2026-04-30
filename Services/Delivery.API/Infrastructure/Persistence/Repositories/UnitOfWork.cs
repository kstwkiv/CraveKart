using Delivery.API.Application.Interfaces;
using Delivery.API.Infrastructure.Persistence;

namespace Delivery.API.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IUnitOfWork"/> for the Delivery bounded context.
/// Coordinates transactional access to delivery and agent repositories.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly DeliveryDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="UnitOfWork"/> and creates the child repositories.
    /// </summary>
    /// <param name="context">The shared database context.</param>
    public UnitOfWork(DeliveryDbContext context)
    {
        _context = context;
        Deliveries = new DeliveryRepository(context);
        Agents = new AgentRepository(context);
    }

    /// <inheritdoc/>
    public IDeliveryRepository Deliveries { get; }

    /// <inheritdoc/>
    public IAgentRepository Agents { get; }

    /// <inheritdoc/>
    public async Task<int> SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}
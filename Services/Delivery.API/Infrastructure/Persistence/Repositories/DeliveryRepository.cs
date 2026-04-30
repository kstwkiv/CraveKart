using Delivery.API.Application.Interfaces;
using Delivery.API.Domain.Entities;
using Delivery.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using DeliveryEntity = Delivery.API.Domain.Entities.Delivery;

namespace Delivery.API.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IDeliveryRepository"/> for managing delivery records.
/// </summary>
public class DeliveryRepository : IDeliveryRepository
{
    private readonly DeliveryDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="DeliveryRepository"/>.
    /// </summary>
    /// <param name="context">The database context to use for data access.</param>
    public DeliveryRepository(DeliveryDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<DeliveryEntity?> GetByOrderIdAsync(Guid orderId) =>
        await _context.Deliveries
            .Include(d => d.Agent)
            .FirstOrDefaultAsync(d => d.OrderId == orderId);

    /// <inheritdoc/>
    public async Task<DeliveryEntity?> GetByAgentIdAsync(Guid agentId) =>
        await _context.Deliveries
            .Include(d => d.Agent)
            .Where(d => d.AgentId == agentId && d.Status == "Assigned")
            .OrderByDescending(d => d.AssignedAt)
            .FirstOrDefaultAsync();

    /// <inheritdoc/>
    public async Task<DeliveryAgent?> GetAvailableAgentAsync() =>
        await _context.DeliveryAgents.FirstOrDefaultAsync(a => a.IsAvailable);

    /// <inheritdoc/>
    public async Task AddAsync(DeliveryEntity delivery) =>
        await _context.Deliveries.AddAsync(delivery);

    /// <inheritdoc/>
    public void Update(DeliveryEntity delivery) =>
        _context.Deliveries.Update(delivery);

    /// <inheritdoc/>
    public void UpdateAgent(DeliveryAgent agent) =>
        _context.DeliveryAgents.Update(agent);
}
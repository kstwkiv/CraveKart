using Delivery.API.Application.Interfaces;
using Delivery.API.Domain.Entities;
using Delivery.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using DeliveryEntity = Delivery.API.Domain.Entities.Delivery;

namespace Delivery.API.Infrastructure.Persistence.Repositories;

public class DeliveryRepository : IDeliveryRepository
{
    private readonly DeliveryDbContext _context;

    public DeliveryRepository(DeliveryDbContext context)
    {
        _context = context;
    }

    public async Task<DeliveryEntity?> GetByOrderIdAsync(Guid orderId) =>
        await _context.Deliveries
            .Include(d => d.Agent)
            .FirstOrDefaultAsync(d => d.OrderId == orderId);

    public async Task<DeliveryEntity?> GetByAgentIdAsync(Guid agentId) =>
        await _context.Deliveries
            .Include(d => d.Agent)
            .Where(d => d.AgentId == agentId && d.Status == "Assigned")
            .OrderByDescending(d => d.AssignedAt)
            .FirstOrDefaultAsync();

    public async Task<DeliveryAgent?> GetAvailableAgentAsync() =>
        await _context.DeliveryAgents.FirstOrDefaultAsync(a => a.IsAvailable);

    public async Task AddAsync(DeliveryEntity delivery) =>
        await _context.Deliveries.AddAsync(delivery);

    public void Update(DeliveryEntity delivery) =>
        _context.Deliveries.Update(delivery);

    public void UpdateAgent(DeliveryAgent agent) =>
        _context.DeliveryAgents.Update(agent);
}
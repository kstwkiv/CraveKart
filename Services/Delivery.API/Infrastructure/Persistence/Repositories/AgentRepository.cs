using Delivery.API.Application.Interfaces;
using Delivery.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Delivery.API.Infrastructure.Persistence.Repositories;

public class AgentRepository : IAgentRepository
{
    private readonly DeliveryDbContext _context;

    public AgentRepository(DeliveryDbContext context)
    {
        _context = context;
    }

    public async Task<DeliveryAgent?> GetByUserIdAsync(Guid userId) =>
        await _context.DeliveryAgents.FirstOrDefaultAsync(a => a.UserId == userId);

    public async Task<DeliveryAgent?> GetByIdAsync(Guid id) =>
        await _context.DeliveryAgents.FindAsync(id);

    public async Task<IEnumerable<DeliveryAgent>> GetAllAsync() =>
        await _context.DeliveryAgents.OrderByDescending(a => a.CreatedAt).ToListAsync();

    public async Task AddAsync(DeliveryAgent agent) =>
        await _context.DeliveryAgents.AddAsync(agent);

    public void Update(DeliveryAgent agent) =>
        _context.DeliveryAgents.Update(agent);
}

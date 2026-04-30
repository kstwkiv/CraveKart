using Delivery.API.Application.Interfaces;
using Delivery.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Delivery.API.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IAgentRepository"/> for managing delivery agent profiles.
/// </summary>
public class AgentRepository : IAgentRepository
{
    private readonly DeliveryDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="AgentRepository"/>.
    /// </summary>
    /// <param name="context">The database context to use for data access.</param>
    public AgentRepository(DeliveryDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<DeliveryAgent?> GetByUserIdAsync(Guid userId) =>
        await _context.DeliveryAgents.FirstOrDefaultAsync(a => a.UserId == userId);

    /// <inheritdoc/>
    public async Task<DeliveryAgent?> GetByIdAsync(Guid id) =>
        await _context.DeliveryAgents.FindAsync(id);

    /// <inheritdoc/>
    public async Task<IEnumerable<DeliveryAgent>> GetAllAsync() =>
        await _context.DeliveryAgents.OrderByDescending(a => a.CreatedAt).ToListAsync();

    /// <inheritdoc/>
    public async Task AddAsync(DeliveryAgent agent) =>
        await _context.DeliveryAgents.AddAsync(agent);

    /// <inheritdoc/>
    public void Update(DeliveryAgent agent) =>
        _context.DeliveryAgents.Update(agent);
}

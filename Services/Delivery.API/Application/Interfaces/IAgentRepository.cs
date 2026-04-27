using Delivery.API.Domain.Entities;

namespace Delivery.API.Application.Interfaces;

public interface IAgentRepository
{
    Task<DeliveryAgent?> GetByUserIdAsync(Guid userId);
    Task<DeliveryAgent?> GetByIdAsync(Guid id);
    Task<IEnumerable<DeliveryAgent>> GetAllAsync();
    Task AddAsync(DeliveryAgent agent);
    void Update(DeliveryAgent agent);
}

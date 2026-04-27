using Delivery.API.Domain.Entities;
using DeliveryEntity = Delivery.API.Domain.Entities.Delivery;

namespace Delivery.API.Application.Interfaces;

public interface IDeliveryRepository
{
    Task<DeliveryEntity?> GetByOrderIdAsync(Guid orderId);
    Task<DeliveryEntity?> GetByAgentIdAsync(Guid agentId);
    Task<DeliveryAgent?> GetAvailableAgentAsync();
    Task AddAsync(DeliveryEntity delivery);
    void Update(DeliveryEntity delivery);
    void UpdateAgent(DeliveryAgent agent);
}
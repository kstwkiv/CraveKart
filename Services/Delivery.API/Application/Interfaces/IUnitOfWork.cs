namespace Delivery.API.Application.Interfaces;

public interface IUnitOfWork
{
    IDeliveryRepository Deliveries { get; }
    IAgentRepository Agents { get; }
    Task<int> SaveChangesAsync();
}
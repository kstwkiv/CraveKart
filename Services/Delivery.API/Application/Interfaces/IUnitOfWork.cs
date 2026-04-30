namespace Delivery.API.Application.Interfaces;

/// <summary>
/// Unit of Work interface for the Delivery bounded context.
/// Coordinates transactional access to delivery and agent repositories.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>Gets the repository for delivery records.</summary>
    IDeliveryRepository Deliveries { get; }

    /// <summary>Gets the repository for delivery agent profiles.</summary>
    IAgentRepository Agents { get; }

    /// <summary>Persists all pending changes to the database.</summary>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync();
}
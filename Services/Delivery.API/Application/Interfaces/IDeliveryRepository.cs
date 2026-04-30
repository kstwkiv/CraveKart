using Delivery.API.Domain.Entities;
using DeliveryEntity = Delivery.API.Domain.Entities.Delivery;

namespace Delivery.API.Application.Interfaces;

/// <summary>
/// Repository interface for managing <see cref="Delivery"/> and related agent persistence operations.
/// </summary>
public interface IDeliveryRepository
{
    /// <summary>Retrieves a delivery record by the associated order ID.</summary>
    /// <param name="orderId">The unique identifier of the order.</param>
    /// <returns>The matching <see cref="DeliveryEntity"/>, or <c>null</c> if not found.</returns>
    Task<DeliveryEntity?> GetByOrderIdAsync(Guid orderId);

    /// <summary>Retrieves the active delivery assigned to a specific agent.</summary>
    /// <param name="agentId">The unique identifier of the delivery agent.</param>
    /// <returns>The active <see cref="DeliveryEntity"/> for the agent, or <c>null</c> if none.</returns>
    Task<DeliveryEntity?> GetByAgentIdAsync(Guid agentId);

    /// <summary>Retrieves the first available (unoccupied) delivery agent.</summary>
    /// <returns>An available <see cref="DeliveryAgent"/>, or <c>null</c> if all agents are busy.</returns>
    Task<DeliveryAgent?> GetAvailableAgentAsync();

    /// <summary>Adds a new delivery record to the repository.</summary>
    /// <param name="delivery">The <see cref="DeliveryEntity"/> to add.</param>
    Task AddAsync(DeliveryEntity delivery);

    /// <summary>Marks an existing delivery record as modified.</summary>
    /// <param name="delivery">The <see cref="DeliveryEntity"/> with updated values.</param>
    void Update(DeliveryEntity delivery);

    /// <summary>Marks an existing delivery agent record as modified.</summary>
    /// <param name="agent">The <see cref="DeliveryAgent"/> with updated values.</param>
    void UpdateAgent(DeliveryAgent agent);
}
using Delivery.API.Domain.Entities;

namespace Delivery.API.Application.Interfaces;

/// <summary>
/// Repository interface for managing <see cref="DeliveryAgent"/> persistence operations.
/// </summary>
public interface IAgentRepository
{
    /// <summary>Retrieves a delivery agent by their associated user account ID.</summary>
    /// <param name="userId">The user account ID linked to the agent profile.</param>
    /// <returns>The matching <see cref="DeliveryAgent"/>, or <c>null</c> if not found.</returns>
    Task<DeliveryAgent?> GetByUserIdAsync(Guid userId);

    /// <summary>Retrieves a delivery agent by their agent profile ID.</summary>
    /// <param name="id">The unique identifier of the agent profile.</param>
    /// <returns>The matching <see cref="DeliveryAgent"/>, or <c>null</c> if not found.</returns>
    Task<DeliveryAgent?> GetByIdAsync(Guid id);

    /// <summary>Retrieves all registered delivery agents ordered by registration date descending.</summary>
    /// <returns>A collection of all <see cref="DeliveryAgent"/> records.</returns>
    Task<IEnumerable<DeliveryAgent>> GetAllAsync();

    /// <summary>Adds a new delivery agent profile to the repository.</summary>
    /// <param name="agent">The <see cref="DeliveryAgent"/> to add.</param>
    Task AddAsync(DeliveryAgent agent);

    /// <summary>Marks an existing delivery agent as modified.</summary>
    /// <param name="agent">The <see cref="DeliveryAgent"/> with updated values.</param>
    void Update(DeliveryAgent agent);
}

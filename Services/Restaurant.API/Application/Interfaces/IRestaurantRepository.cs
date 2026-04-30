using Restaurant.API.Domain.Entities;
using Restaurant.API.Domain.Enums;

namespace Restaurant.API.Application.Interfaces;

/// <summary>
/// Repository interface for managing <see cref="Restaurant"/> persistence operations.
/// </summary>
public interface IRestaurantRepository
{
    /// <summary>Retrieves all restaurants with Active status.</summary>
    /// <returns>A collection of active <see cref="Restaurant"/> records.</returns>
    Task<IEnumerable<Domain.Entities.Restaurant>> GetAllActiveAsync();

    /// <summary>Retrieves all restaurants regardless of status.</summary>
    /// <returns>A collection of all <see cref="Restaurant"/> records.</returns>
    Task<IEnumerable<Domain.Entities.Restaurant>> GetAllAsync();

    /// <summary>Retrieves all restaurants with a specific status.</summary>
    /// <param name="status">The status to filter by.</param>
    /// <returns>A collection of <see cref="Restaurant"/> records matching the status.</returns>
    Task<IEnumerable<Domain.Entities.Restaurant>> GetByStatusAsync(RestaurantStatus status);

    /// <summary>Searches active restaurants by name or cuisine type.</summary>
    /// <param name="term">The search term to match against name and cuisine types.</param>
    /// <returns>A collection of matching <see cref="Restaurant"/> records.</returns>
    Task<IEnumerable<Domain.Entities.Restaurant>> SearchAsync(string term);

    /// <summary>Retrieves a restaurant by its unique identifier, including menu categories and items.</summary>
    /// <param name="id">The unique identifier of the restaurant.</param>
    /// <returns>The matching <see cref="Restaurant"/>, or <c>null</c> if not found.</returns>
    Task<Domain.Entities.Restaurant?> GetByIdAsync(Guid id);

    /// <summary>Retrieves the first restaurant owned by a specific user.</summary>
    /// <param name="ownerId">The unique identifier of the owner.</param>
    /// <returns>The matching <see cref="Restaurant"/>, or <c>null</c> if not found.</returns>
    Task<Domain.Entities.Restaurant?> GetByOwnerIdAsync(Guid ownerId);

    /// <summary>Retrieves all restaurants owned by a specific user.</summary>
    /// <param name="ownerId">The unique identifier of the owner.</param>
    /// <returns>A collection of <see cref="Restaurant"/> records owned by the user.</returns>
    Task<IEnumerable<Domain.Entities.Restaurant>> GetAllByOwnerIdAsync(Guid ownerId);

    /// <summary>Adds a new restaurant to the repository.</summary>
    /// <param name="restaurant">The <see cref="Restaurant"/> to add.</param>
    Task AddAsync(Domain.Entities.Restaurant restaurant);

    /// <summary>Marks an existing restaurant as modified.</summary>
    /// <param name="restaurant">The <see cref="Restaurant"/> with updated values.</param>
    void Update(Domain.Entities.Restaurant restaurant);
}
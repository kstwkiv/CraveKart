using Restaurant.API.Application.Commands;
using Restaurant.API.Application.DTOs;

namespace Restaurant.API.Application.Interfaces;

/// <summary>
/// Service interface defining the core restaurant management operations.
/// </summary>
public interface IRestaurantService
{
    /// <summary>Retrieves all active restaurants, optionally filtered by a search term.</summary>
    /// <param name="searchTerm">Optional search term to filter by name or cuisine type.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of <see cref="RestaurantDto"/> records.</returns>
    Task<List<RestaurantDto>> GetAllAsync(string? searchTerm, CancellationToken cancellationToken = default);

    /// <summary>Retrieves a single restaurant by its unique identifier.</summary>
    /// <param name="restaurantId">The unique identifier of the restaurant.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The <see cref="RestaurantDto"/> if found; otherwise <c>null</c>.</returns>
    Task<RestaurantDto?> GetByIdAsync(Guid restaurantId, CancellationToken cancellationToken = default);

    /// <summary>Creates a new restaurant listing with Pending status.</summary>
    /// <param name="request">The command containing restaurant details.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The created <see cref="RestaurantDto"/>.</returns>
    Task<RestaurantDto> CreateAsync(CreateRestaurantCommand request, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing restaurant. Returns null if not found or ownership check fails.</summary>
    /// <param name="restaurantId">The unique identifier of the restaurant to update.</param>
    /// <param name="requestingOwnerId">The ID of the owner making the request (for ownership validation).</param>
    /// <param name="request">The command containing updated restaurant details.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The updated <see cref="RestaurantDto"/>, or <c>null</c> if not found or unauthorized.</returns>
    Task<RestaurantDto?> UpdateAsync(Guid restaurantId, Guid requestingOwnerId, UpdateRestaurantCommand request, CancellationToken cancellationToken = default);

    /// <summary>Toggles the open/closed availability status of a restaurant.</summary>
    /// <param name="restaurantId">The unique identifier of the restaurant.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The new <c>IsOpen</c> state, or <c>null</c> if the restaurant was not found.</returns>
    Task<bool?> ToggleAvailabilityAsync(Guid restaurantId, CancellationToken cancellationToken = default);

    /// <summary>Retrieves all menu items for a restaurant, flattened across all categories.</summary>
    /// <param name="restaurantId">The unique identifier of the restaurant.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of <see cref="MenuItemDto"/> records.</returns>
    Task<List<MenuItemDto>> GetMenuAsync(Guid restaurantId, CancellationToken cancellationToken = default);

    /// <summary>Creates a new menu item within a category.</summary>
    /// <param name="request">The command containing menu item details.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The created <see cref="MenuItemDto"/>.</returns>
    Task<MenuItemDto> CreateMenuItemAsync(CreateMenuItemCommand request, CancellationToken cancellationToken = default);
}

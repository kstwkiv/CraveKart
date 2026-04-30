using Restaurant.API.Application.DTOs;

namespace Restaurant.API.Application.Queries;

/// <summary>
/// Query to retrieve all menu items for a specific restaurant, grouped by category.
/// </summary>
/// <param name="RestaurantId">The unique identifier of the restaurant.</param>
public record GetMenuByRestaurantQuery(Guid RestaurantId);

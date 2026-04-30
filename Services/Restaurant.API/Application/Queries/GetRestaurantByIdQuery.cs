using Restaurant.API.Application.DTOs;

namespace Restaurant.API.Application.Queries;

/// <summary>
/// Query to retrieve a single restaurant by its unique identifier.
/// </summary>
/// <param name="RestaurantId">The unique identifier of the restaurant to retrieve.</param>
public record GetRestaurantByIdQuery(Guid RestaurantId);

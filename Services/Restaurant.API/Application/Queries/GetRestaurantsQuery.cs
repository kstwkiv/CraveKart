using Restaurant.API.Application.DTOs;

namespace Restaurant.API.Application.Queries;

/// <summary>
/// Query to retrieve all active restaurants, optionally filtered by a search term.
/// </summary>
/// <param name="SearchTerm">Optional search term to filter by restaurant name or cuisine type.</param>
public record GetRestaurantsQuery(string? SearchTerm);

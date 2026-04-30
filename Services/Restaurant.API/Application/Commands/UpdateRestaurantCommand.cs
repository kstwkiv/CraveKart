using Restaurant.API.Application.DTOs;

namespace Restaurant.API.Application.Commands;

/// <summary>
/// Command to update the details of an existing restaurant.
/// Only the owning restaurant owner can update their restaurant.
/// </summary>
/// <param name="RestaurantId">The unique identifier of the restaurant to update.</param>
/// <param name="Name">The new display name of the restaurant.</param>
/// <param name="Description">The new description of the restaurant.</param>
/// <param name="Address">The new physical address of the restaurant.</param>
/// <param name="CuisineTypes">The updated comma-separated cuisine types.</param>
/// <param name="OperatingHours">The updated operating hours description.</param>
/// <param name="MinimumOrderAmount">The updated minimum order amount.</param>
/// <param name="EstimatedDeliveryMinutes">The updated estimated delivery time in minutes.</param>
/// <param name="LogoUrl">Optional updated URL of the restaurant's logo image.</param>
public record UpdateRestaurantCommand(
    Guid RestaurantId,
    string Name,
    string Description,
    string Address,
    string CuisineTypes,
    string OperatingHours,
    double MinimumOrderAmount,
    int EstimatedDeliveryMinutes,
    string? LogoUrl);

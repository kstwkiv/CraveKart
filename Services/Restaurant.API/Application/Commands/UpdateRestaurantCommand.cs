using Restaurant.API.Application.DTOs;

namespace Restaurant.API.Application.Commands;

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

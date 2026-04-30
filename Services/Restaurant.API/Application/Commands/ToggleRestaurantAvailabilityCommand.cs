namespace Restaurant.API.Application.Commands;

/// <summary>
/// Command to toggle the open/closed availability status of a restaurant.
/// </summary>
/// <param name="RestaurantId">The unique identifier of the restaurant.</param>
/// <param name="IsOpen">The new availability state to set.</param>
public record ToggleRestaurantAvailabilityCommand(Guid RestaurantId, bool IsOpen);

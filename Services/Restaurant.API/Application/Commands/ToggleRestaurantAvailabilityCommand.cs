namespace Restaurant.API.Application.Commands;

public record ToggleRestaurantAvailabilityCommand(Guid RestaurantId, bool IsOpen);

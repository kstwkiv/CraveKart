namespace Restaurant.API.Application.Commands;

/// <summary>
/// Command to register a new restaurant on the platform.
/// The restaurant is created with a Pending status awaiting admin approval.
/// </summary>
/// <param name="OwnerId">The unique identifier of the restaurant owner.</param>
/// <param name="OwnerEmail">The email address of the restaurant owner.</param>
/// <param name="Name">The display name of the restaurant.</param>
/// <param name="Description">A description of the restaurant.</param>
/// <param name="Address">The physical address of the restaurant.</param>
/// <param name="Lat">The latitude coordinate of the restaurant location.</param>
/// <param name="Lng">The longitude coordinate of the restaurant location.</param>
/// <param name="CuisineTypes">Comma-separated cuisine types (e.g., "Indian,Chinese").</param>
/// <param name="OperatingHours">The operating hours description (e.g., "9 AM - 10 PM").</param>
/// <param name="MinimumOrderAmount">The minimum order amount required.</param>
/// <param name="EstimatedDeliveryMinutes">The estimated delivery time in minutes.</param>
/// <param name="LogoUrl">Optional URL of the restaurant's logo image.</param>
public record CreateRestaurantCommand(
    Guid OwnerId,
    string OwnerEmail,
    string Name,
    string Description,
    string Address,
    double Lat,
    double Lng,
    string CuisineTypes,
    string OperatingHours,
    double MinimumOrderAmount,
    int EstimatedDeliveryMinutes,
    string? LogoUrl = null);

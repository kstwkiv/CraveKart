namespace Restaurant.API.Application.DTOs;

/// <summary>
/// Request DTO for creating a new restaurant listing.
/// </summary>
public class CreateRestaurantRequest
{
    /// <summary>Gets or sets the display name of the restaurant.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the description of the restaurant.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the physical address of the restaurant.</summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>Gets or sets the latitude coordinate of the restaurant location.</summary>
    public double Lat { get; set; }

    /// <summary>Gets or sets the longitude coordinate of the restaurant location.</summary>
    public double Lng { get; set; }

    /// <summary>Gets or sets comma-separated cuisine types (e.g., "Indian,Chinese").</summary>
    public string CuisineTypes { get; set; } = string.Empty;

    /// <summary>Gets or sets the operating hours description (e.g., "9 AM - 10 PM").</summary>
    public string OperatingHours { get; set; } = string.Empty;

    /// <summary>Gets or sets the minimum order amount required.</summary>
    public double MinimumOrderAmount { get; set; }

    /// <summary>Gets or sets the estimated delivery time in minutes.</summary>
    public int EstimatedDeliveryMinutes { get; set; }

    /// <summary>Gets or sets the optional URL of the restaurant's logo image.</summary>
    public string? LogoUrl { get; set; }
}
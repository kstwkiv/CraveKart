namespace Restaurant.API.Application.DTOs;

/// <summary>
/// Data transfer object representing a restaurant returned to API consumers.
/// </summary>
public class RestaurantDto
{
    /// <summary>Gets or sets the unique identifier of the restaurant.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the unique identifier of the restaurant owner.</summary>
    public Guid OwnerId { get; set; }

    /// <summary>Gets or sets the display name of the restaurant.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the description of the restaurant.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the physical address of the restaurant.</summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>Gets or sets comma-separated cuisine types offered by the restaurant.</summary>
    public string CuisineTypes { get; set; } = string.Empty;

    /// <summary>Gets or sets the operating hours description.</summary>
    public string? OperatingHours { get; set; }

    /// <summary>Gets or sets the average customer rating (0–5).</summary>
    public double AverageRating { get; set; }

    /// <summary>Gets or sets the total number of customer reviews.</summary>
    public int TotalReviews { get; set; }

    /// <summary>Gets or sets a value indicating whether the restaurant is currently open for orders.</summary>
    public bool IsOpen { get; set; }

    /// <summary>Gets or sets the estimated delivery time in minutes.</summary>
    public int EstimatedDeliveryMinutes { get; set; }

    /// <summary>Gets or sets the minimum order amount required.</summary>
    public double MinimumOrderAmount { get; set; }

    /// <summary>Gets or sets the current approval status of the restaurant (e.g., "Pending", "Active").</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional URL of the restaurant's logo image.</summary>
    public string? LogoUrl { get; set; }
}
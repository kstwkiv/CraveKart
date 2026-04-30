using Restaurant.API.Domain.Enums;

namespace Restaurant.API.Domain.Entities;

/// <summary>
/// Represents a restaurant entity in the Restaurant bounded context.
/// </summary>
public class Restaurant
{
    /// <summary>Gets or sets the unique identifier of the restaurant.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Gets or sets the unique identifier of the restaurant owner.</summary>
    public Guid OwnerId { get; set; }

    /// <summary>Gets or sets the email address of the restaurant owner.</summary>
    public string OwnerEmail { get; set; } = string.Empty;

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

    /// <summary>Gets or sets comma-separated cuisine types offered by the restaurant.</summary>
    public string CuisineTypes { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional URL of the restaurant's logo image.</summary>
    public string? LogoUrl { get; set; }

    /// <summary>Gets or sets the average customer rating (0–5).</summary>
    public double AverageRating { get; set; } = 0;

    /// <summary>Gets or sets the total number of customer reviews.</summary>
    public int TotalReviews { get; set; } = 0;

    /// <summary>Gets or sets the current approval status of the restaurant.</summary>
    public RestaurantStatus Status { get; set; } = RestaurantStatus.Pending;

    /// <summary>Gets or sets a value indicating whether the restaurant is currently open for orders.</summary>
    public bool IsOpen { get; set; } = false;

    /// <summary>Gets or sets the operating hours description (e.g., "9 AM - 10 PM").</summary>
    public string OperatingHours { get; set; } = string.Empty;

    /// <summary>Gets or sets the minimum order amount required.</summary>
    public double MinimumOrderAmount { get; set; }

    /// <summary>Gets or sets the estimated delivery time in minutes.</summary>
    public int EstimatedDeliveryMinutes { get; set; }

    /// <summary>Gets or sets the IST timestamp when the restaurant was created.</summary>
    public DateTime CreatedAt { get; set; } = IstClock.Now;

    /// <summary>Gets or sets the IST timestamp when the restaurant was last updated.</summary>
    public DateTime UpdatedAt { get; set; } = IstClock.Now;

    /// <summary>Gets or sets the collection of menu categories for this restaurant.</summary>
    public ICollection<MenuCategory> MenuCategories { get; set; } = new List<MenuCategory>();
}
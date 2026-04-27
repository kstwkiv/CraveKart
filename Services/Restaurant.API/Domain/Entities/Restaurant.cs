using Restaurant.API.Domain.Enums;

namespace Restaurant.API.Domain.Entities;

public class Restaurant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OwnerId { get; set; }
    public string OwnerEmail { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double Lat { get; set; }
    public double Lng { get; set; }
    public string CuisineTypes { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public double AverageRating { get; set; } = 0;
    public int TotalReviews { get; set; } = 0;
    public RestaurantStatus Status { get; set; } = RestaurantStatus.Pending;
    public bool IsOpen { get; set; } = false;
    public string OperatingHours { get; set; } = string.Empty;
    public double MinimumOrderAmount { get; set; }
    public int EstimatedDeliveryMinutes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<MenuCategory> MenuCategories { get; set; } = new List<MenuCategory>();
}
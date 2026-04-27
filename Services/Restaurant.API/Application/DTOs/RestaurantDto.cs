namespace Restaurant.API.Application.DTOs;

public class RestaurantDto
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string CuisineTypes { get; set; } = string.Empty;
    public string? OperatingHours { get; set; }
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public bool IsOpen { get; set; }
    public int EstimatedDeliveryMinutes { get; set; }
    public double MinimumOrderAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
}
namespace Restaurant.API.Application.DTOs;

public class CreateRestaurantRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double Lat { get; set; }
    public double Lng { get; set; }
    public string CuisineTypes { get; set; } = string.Empty;
    public string OperatingHours { get; set; } = string.Empty;
    public double MinimumOrderAmount { get; set; }
    public int EstimatedDeliveryMinutes { get; set; }
    public string? LogoUrl { get; set; }
}
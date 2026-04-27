namespace Restaurant.API.Application.DTOs;

public class CreateMenuItemRequest
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string DietaryTags { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}
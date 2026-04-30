namespace Restaurant.API.Application.DTOs;

/// <summary>
/// Request DTO for creating a new menu item within a category.
/// </summary>
public class CreateMenuItemRequest
{
    /// <summary>Gets or sets the unique identifier of the menu category to add the item to.</summary>
    public Guid CategoryId { get; set; }

    /// <summary>Gets or sets the display name of the menu item.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the description of the menu item.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the price of the menu item.</summary>
    public decimal Price { get; set; }

    /// <summary>Gets or sets comma-separated dietary tags (e.g., "Veg,Gluten-Free").</summary>
    public string DietaryTags { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional URL of the menu item's image.</summary>
    public string? ImageUrl { get; set; }
}
namespace Restaurant.API.Application.DTOs;

/// <summary>
/// Data transfer object representing a menu item returned to API consumers.
/// </summary>
public class MenuItemDto
{
    /// <summary>Gets or sets the unique identifier of the menu item.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the unique identifier of the parent menu category.</summary>
    public Guid CategoryId { get; set; }

    /// <summary>Gets or sets the display name of the menu item.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the description of the menu item.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the price of the menu item.</summary>
    public decimal Price { get; set; }

    /// <summary>Gets or sets the optional URL of the menu item's image.</summary>
    public string? ImageUrl { get; set; }

    /// <summary>Gets or sets a value indicating whether the menu item is currently available for ordering.</summary>
    public bool IsAvailable { get; set; }

    /// <summary>Gets or sets comma-separated dietary tags (e.g., "Veg,Gluten-Free").</summary>
    public string DietaryTags { get; set; } = string.Empty;
}
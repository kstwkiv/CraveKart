namespace Restaurant.API.Domain.Entities;

/// <summary>
/// Represents a single menu item available for ordering within a menu category.
/// </summary>
public class MenuItem
{
    /// <summary>Gets or sets the unique identifier of the menu item.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

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
    public bool IsAvailable { get; set; } = true;

    /// <summary>Gets or sets comma-separated dietary tags (e.g., "Veg,Gluten-Free").</summary>
    public string DietaryTags { get; set; } = string.Empty;

    /// <summary>Gets or sets the IST timestamp when the menu item was created.</summary>
    public DateTime CreatedAt { get; set; } = IstClock.Now;

    /// <summary>Gets or sets the parent menu category (navigation property).</summary>
    public MenuCategory Category { get; set; } = null!;
}
namespace Restaurant.API.Domain.Entities;

/// <summary>
/// Represents a menu category grouping related menu items within a restaurant.
/// </summary>
public class MenuCategory
{
    /// <summary>Gets or sets the unique identifier of the menu category.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Gets or sets the unique identifier of the parent restaurant.</summary>
    public Guid RestaurantId { get; set; }

    /// <summary>Gets or sets the display name of the menu category.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the sort order for displaying this category. Lower values appear first.</summary>
    public int SortOrder { get; set; }

    /// <summary>Gets or sets the IST timestamp when the category was created.</summary>
    public DateTime CreatedAt { get; set; } = IstClock.Now;

    /// <summary>Gets or sets the parent restaurant (navigation property).</summary>
    public Restaurant Restaurant { get; set; } = null!;

    /// <summary>Gets or sets the collection of menu items within this category.</summary>
    public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
}
namespace Restaurant.API.Application.DTOs;

/// <summary>
/// Data transfer object representing a menu category with its items.
/// </summary>
public class MenuCategoryDto
{
    /// <summary>Gets or sets the unique identifier of the menu category.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the display name of the menu category.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the list of menu items within this category.</summary>
    public List<MenuItemDto> Items { get; set; } = new();
}

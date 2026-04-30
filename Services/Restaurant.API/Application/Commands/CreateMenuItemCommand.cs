namespace Restaurant.API.Application.Commands;

/// <summary>
/// Command to create a new menu item within a specific menu category.
/// </summary>
/// <param name="CategoryId">The unique identifier of the menu category to add the item to.</param>
/// <param name="Name">The display name of the menu item.</param>
/// <param name="Description">A description of the menu item.</param>
/// <param name="Price">The price of the menu item.</param>
/// <param name="DietaryTags">Comma-separated dietary tags (e.g., "Veg,Gluten-Free").</param>
/// <param name="ImageUrl">Optional URL of the menu item's image.</param>
public record CreateMenuItemCommand(
    Guid CategoryId,
    string Name,
    string Description,
    decimal Price,
    string DietaryTags,
    string? ImageUrl = null);

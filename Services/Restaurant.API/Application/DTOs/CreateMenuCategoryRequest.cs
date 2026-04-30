namespace Restaurant.API.Application.DTOs;

/// <summary>
/// Request DTO for creating a new menu category within a restaurant.
/// </summary>
public class CreateMenuCategoryRequest
{
    /// <summary>Gets or sets the display name of the menu category.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the sort order for displaying this category. Lower values appear first.</summary>
    public int SortOrder { get; set; } = 0;
}

/// <summary>
/// Request DTO for partially updating an existing menu item.
/// All properties are optional; only provided values will be updated.
/// </summary>
public class UpdateMenuItemRequest
{
    /// <summary>Gets or sets the new display name of the menu item. Null to keep existing.</summary>
    public string? Name { get; set; }

    /// <summary>Gets or sets the new description of the menu item. Null to keep existing.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the new price of the menu item. Null to keep existing.</summary>
    public decimal? Price { get; set; }

    /// <summary>Gets or sets the new availability status of the menu item. Null to keep existing.</summary>
    public bool? IsAvailable { get; set; }

    /// <summary>Gets or sets the new image URL of the menu item. Null to keep existing.</summary>
    public string? ImageUrl { get; set; }

    /// <summary>Gets or sets the new dietary tags for the menu item. Null to keep existing.</summary>
    public string? DietaryTags { get; set; }
}

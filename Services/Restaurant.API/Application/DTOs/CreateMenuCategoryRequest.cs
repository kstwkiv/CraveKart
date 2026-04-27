namespace Restaurant.API.Application.DTOs;

public class CreateMenuCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; } = 0;
}

public class UpdateMenuItemRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public bool? IsAvailable { get; set; }
    public string? ImageUrl { get; set; }
    public string? DietaryTags { get; set; }
}

namespace Restaurant.API.Application.DTOs;

public class MenuCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<MenuItemDto> Items { get; set; } = new();
}

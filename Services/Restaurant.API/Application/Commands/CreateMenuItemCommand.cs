namespace Restaurant.API.Application.Commands;

public record CreateMenuItemCommand(
    Guid CategoryId,
    string Name,
    string Description,
    decimal Price,
    string DietaryTags,
    string? ImageUrl = null);

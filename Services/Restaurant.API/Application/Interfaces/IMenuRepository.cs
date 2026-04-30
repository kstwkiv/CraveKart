using Restaurant.API.Domain.Entities;

namespace Restaurant.API.Application.Interfaces;

/// <summary>
/// Repository interface for managing <see cref="MenuCategory"/> and <see cref="MenuItem"/> persistence.
/// </summary>
public interface IMenuRepository
{
    /// <summary>Retrieves all menu categories with their items for a specific restaurant.</summary>
    /// <param name="restaurantId">The unique identifier of the restaurant.</param>
    /// <returns>A collection of <see cref="MenuCategory"/> records with items included.</returns>
    Task<IEnumerable<MenuCategory>> GetCategoriesWithItemsAsync(Guid restaurantId);

    /// <summary>Adds a new menu category to the repository.</summary>
    /// <param name="category">The <see cref="MenuCategory"/> to add.</param>
    Task AddCategoryAsync(MenuCategory category);

    /// <summary>Adds a new menu item to the repository.</summary>
    /// <param name="item">The <see cref="MenuItem"/> to add.</param>
    Task AddItemAsync(MenuItem item);

    /// <summary>Retrieves a menu category by its unique identifier.</summary>
    /// <param name="categoryId">The unique identifier of the category.</param>
    /// <returns>The matching <see cref="MenuCategory"/>, or <c>null</c> if not found.</returns>
    Task<MenuCategory?> GetCategoryByIdAsync(Guid categoryId);

    /// <summary>Retrieves a menu item by its unique identifier.</summary>
    /// <param name="itemId">The unique identifier of the menu item.</param>
    /// <returns>The matching <see cref="MenuItem"/>, or <c>null</c> if not found.</returns>
    Task<MenuItem?> GetItemByIdAsync(Guid itemId);

    /// <summary>Marks an existing menu item as modified.</summary>
    /// <param name="item">The <see cref="MenuItem"/> with updated values.</param>
    void UpdateItem(MenuItem item);

    /// <summary>Removes a menu item from the repository.</summary>
    /// <param name="item">The <see cref="MenuItem"/> to delete.</param>
    void DeleteItem(MenuItem item);
}

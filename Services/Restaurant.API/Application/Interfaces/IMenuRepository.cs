using Restaurant.API.Domain.Entities;

namespace Restaurant.API.Application.Interfaces;

public interface IMenuRepository
{
    Task<IEnumerable<MenuCategory>> GetCategoriesWithItemsAsync(Guid restaurantId);
    Task AddCategoryAsync(MenuCategory category);
    Task AddItemAsync(MenuItem item);
    Task<MenuCategory?> GetCategoryByIdAsync(Guid categoryId);
    Task<MenuItem?> GetItemByIdAsync(Guid itemId);
    void UpdateItem(MenuItem item);
    void DeleteItem(MenuItem item);
}

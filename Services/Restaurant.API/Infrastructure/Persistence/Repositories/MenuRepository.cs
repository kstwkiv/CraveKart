using Microsoft.EntityFrameworkCore;
using Restaurant.API.Application.Interfaces;
using Restaurant.API.Domain.Entities;

namespace Restaurant.API.Infrastructure.Persistence.Repositories;

public class MenuRepository : IMenuRepository
{
    private readonly RestaurantDbContext _context;

    public MenuRepository(RestaurantDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MenuCategory>> GetCategoriesWithItemsAsync(Guid restaurantId) =>
        await _context.MenuCategories
            .Include(c => c.MenuItems)
            .Where(c => c.RestaurantId == restaurantId)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();

    public async Task AddCategoryAsync(MenuCategory category) =>
        await _context.MenuCategories.AddAsync(category);

    public async Task AddItemAsync(MenuItem item) =>
        await _context.MenuItems.AddAsync(item);

    public async Task<MenuCategory?> GetCategoryByIdAsync(Guid categoryId) =>
        await _context.MenuCategories.FindAsync(categoryId);

    public async Task<MenuItem?> GetItemByIdAsync(Guid itemId) =>
        await _context.MenuItems.FindAsync(itemId);

    public void UpdateItem(MenuItem item) =>
        _context.MenuItems.Update(item);

    public void DeleteItem(MenuItem item) =>
        _context.MenuItems.Remove(item);
}

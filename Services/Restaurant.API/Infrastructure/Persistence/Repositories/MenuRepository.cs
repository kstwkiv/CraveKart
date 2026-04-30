using Microsoft.EntityFrameworkCore;
using Restaurant.API.Application.Interfaces;
using Restaurant.API.Domain.Entities;

namespace Restaurant.API.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IMenuRepository"/> for managing menu categories and items.
/// </summary>
public class MenuRepository : IMenuRepository
{
    private readonly RestaurantDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="MenuRepository"/>.
    /// </summary>
    /// <param name="context">The database context to use for data access.</param>
    public MenuRepository(RestaurantDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<MenuCategory>> GetCategoriesWithItemsAsync(Guid restaurantId) =>
        await _context.MenuCategories
            .Include(c => c.MenuItems)
            .Where(c => c.RestaurantId == restaurantId)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();

    /// <inheritdoc/>
    public async Task AddCategoryAsync(MenuCategory category) =>
        await _context.MenuCategories.AddAsync(category);

    /// <inheritdoc/>
    public async Task AddItemAsync(MenuItem item) =>
        await _context.MenuItems.AddAsync(item);

    /// <inheritdoc/>
    public async Task<MenuCategory?> GetCategoryByIdAsync(Guid categoryId) =>
        await _context.MenuCategories.FindAsync(categoryId);

    /// <inheritdoc/>
    public async Task<MenuItem?> GetItemByIdAsync(Guid itemId) =>
        await _context.MenuItems.FindAsync(itemId);

    /// <inheritdoc/>
    public void UpdateItem(MenuItem item) =>
        _context.MenuItems.Update(item);

    /// <inheritdoc/>
    public void DeleteItem(MenuItem item) =>
        _context.MenuItems.Remove(item);
}

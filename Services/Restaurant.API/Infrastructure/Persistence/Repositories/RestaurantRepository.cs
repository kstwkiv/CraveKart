using Microsoft.EntityFrameworkCore;
using Restaurant.API.Application.Interfaces;
using Restaurant.API.Domain.Enums;

namespace Restaurant.API.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IRestaurantRepository"/> for managing restaurant records.
/// </summary>
public class RestaurantRepository : IRestaurantRepository
{
    private readonly RestaurantDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="RestaurantRepository"/>.
    /// </summary>
    /// <param name="context">The database context to use for data access.</param>
    public RestaurantRepository(RestaurantDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Domain.Entities.Restaurant>> GetAllActiveAsync() =>
        await _context.Restaurants
            .Where(r => r.Status == RestaurantStatus.Active)
            .ToListAsync();

    /// <inheritdoc/>
    public async Task<IEnumerable<Domain.Entities.Restaurant>> GetAllAsync() =>
        await _context.Restaurants.OrderByDescending(r => r.CreatedAt).ToListAsync();

    /// <inheritdoc/>
    public async Task<IEnumerable<Domain.Entities.Restaurant>> GetByStatusAsync(RestaurantStatus status) =>
        await _context.Restaurants
            .Where(r => r.Status == status)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    /// <inheritdoc/>
    public async Task<IEnumerable<Domain.Entities.Restaurant>> SearchAsync(string term) =>
        await _context.Restaurants
            .Where(r => r.Status == RestaurantStatus.Active &&
                       (r.Name.Contains(term) || r.CuisineTypes.Contains(term)))
            .ToListAsync();

    /// <inheritdoc/>
    public async Task<Domain.Entities.Restaurant?> GetByIdAsync(Guid id) =>
        await _context.Restaurants
            .Include(r => r.MenuCategories)
            .ThenInclude(c => c.MenuItems)
            .FirstOrDefaultAsync(r => r.Id == id);

    /// <inheritdoc/>
    public async Task<Domain.Entities.Restaurant?> GetByOwnerIdAsync(Guid ownerId) =>
        await _context.Restaurants
            .FirstOrDefaultAsync(r => r.OwnerId == ownerId);

    /// <inheritdoc/>
    public async Task<IEnumerable<Domain.Entities.Restaurant>> GetAllByOwnerIdAsync(Guid ownerId) =>
        await _context.Restaurants
            .Where(r => r.OwnerId == ownerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    /// <inheritdoc/>
    public async Task AddAsync(Domain.Entities.Restaurant restaurant) =>
        await _context.Restaurants.AddAsync(restaurant);

    /// <inheritdoc/>
    public void Update(Domain.Entities.Restaurant restaurant) =>
        _context.Restaurants.Update(restaurant);
}
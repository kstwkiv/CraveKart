using Microsoft.EntityFrameworkCore;
using Restaurant.API.Application.Interfaces;
using Restaurant.API.Domain.Enums;

namespace Restaurant.API.Infrastructure.Persistence.Repositories;

public class RestaurantRepository : IRestaurantRepository
{
    private readonly RestaurantDbContext _context;

    public RestaurantRepository(RestaurantDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Domain.Entities.Restaurant>> GetAllActiveAsync() =>
        await _context.Restaurants
            .Where(r => r.Status == RestaurantStatus.Active)
            .ToListAsync();

    public async Task<IEnumerable<Domain.Entities.Restaurant>> GetAllAsync() =>
        await _context.Restaurants.OrderByDescending(r => r.CreatedAt).ToListAsync();

    public async Task<IEnumerable<Domain.Entities.Restaurant>> GetByStatusAsync(RestaurantStatus status) =>
        await _context.Restaurants
            .Where(r => r.Status == status)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Domain.Entities.Restaurant>> SearchAsync(string term) =>
        await _context.Restaurants
            .Where(r => r.Status == RestaurantStatus.Active &&
                       (r.Name.Contains(term) || r.CuisineTypes.Contains(term)))
            .ToListAsync();

    public async Task<Domain.Entities.Restaurant?> GetByIdAsync(Guid id) =>
        await _context.Restaurants
            .Include(r => r.MenuCategories)
            .ThenInclude(c => c.MenuItems)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<Domain.Entities.Restaurant?> GetByOwnerIdAsync(Guid ownerId) =>
        await _context.Restaurants
            .FirstOrDefaultAsync(r => r.OwnerId == ownerId);

    public async Task<IEnumerable<Domain.Entities.Restaurant>> GetAllByOwnerIdAsync(Guid ownerId) =>
        await _context.Restaurants
            .Where(r => r.OwnerId == ownerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public async Task AddAsync(Domain.Entities.Restaurant restaurant) =>
        await _context.Restaurants.AddAsync(restaurant);

    public void Update(Domain.Entities.Restaurant restaurant) =>
        _context.Restaurants.Update(restaurant);
}
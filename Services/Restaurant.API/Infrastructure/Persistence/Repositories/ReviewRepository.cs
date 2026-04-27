using Microsoft.EntityFrameworkCore;
using Restaurant.API.Application.Interfaces;
using Restaurant.API.Domain.Entities;

namespace Restaurant.API.Infrastructure.Persistence.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly RestaurantDbContext _context;

    public ReviewRepository(RestaurantDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Review>> GetByRestaurantIdAsync(Guid restaurantId) =>
        await _context.Reviews
            .Where(r => r.RestaurantId == restaurantId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public async Task<Review?> GetByOrderIdAsync(Guid orderId) =>
        await _context.Reviews.FirstOrDefaultAsync(r => r.OrderId == orderId);

    public async Task<bool> ExistsForOrderAsync(Guid orderId, Guid customerId) =>
        await _context.Reviews.AnyAsync(r => r.OrderId == orderId && r.CustomerId == customerId);

    public async Task AddAsync(Review review) =>
        await _context.Reviews.AddAsync(review);

    public async Task<Review?> GetByIdAsync(Guid id) =>
        await _context.Reviews.FindAsync(id);

    public void Update(Review review) =>
        _context.Reviews.Update(review);

    public void Delete(Review review) =>
        _context.Reviews.Remove(review);
}

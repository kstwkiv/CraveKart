using Microsoft.EntityFrameworkCore;
using Restaurant.API.Application.Interfaces;
using Restaurant.API.Domain.Entities;

namespace Restaurant.API.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IReviewRepository"/> for managing customer reviews.
/// </summary>
public class ReviewRepository : IReviewRepository
{
    private readonly RestaurantDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="ReviewRepository"/>.
    /// </summary>
    /// <param name="context">The database context to use for data access.</param>
    public ReviewRepository(RestaurantDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Review>> GetByRestaurantIdAsync(Guid restaurantId) =>
        await _context.Reviews
            .Where(r => r.RestaurantId == restaurantId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    /// <inheritdoc/>
    public async Task<Review?> GetByOrderIdAsync(Guid orderId) =>
        await _context.Reviews.FirstOrDefaultAsync(r => r.OrderId == orderId);

    /// <inheritdoc/>
    public async Task<bool> ExistsForOrderAsync(Guid orderId, Guid customerId) =>
        await _context.Reviews.AnyAsync(r => r.OrderId == orderId && r.CustomerId == customerId);

    /// <inheritdoc/>
    public async Task AddAsync(Review review) =>
        await _context.Reviews.AddAsync(review);

    /// <inheritdoc/>
    public async Task<Review?> GetByIdAsync(Guid id) =>
        await _context.Reviews.FindAsync(id);

    /// <inheritdoc/>
    public void Update(Review review) =>
        _context.Reviews.Update(review);

    /// <inheritdoc/>
    public void Delete(Review review) =>
        _context.Reviews.Remove(review);
}

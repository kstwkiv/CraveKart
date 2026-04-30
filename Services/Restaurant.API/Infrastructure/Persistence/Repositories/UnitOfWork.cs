using Restaurant.API.Application.Interfaces;

namespace Restaurant.API.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IUnitOfWork"/> for the Restaurant bounded context.
/// Coordinates transactional access to restaurant, menu, and review repositories.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly RestaurantDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="UnitOfWork"/> and creates the child repositories.
    /// </summary>
    /// <param name="context">The shared database context.</param>
    public UnitOfWork(RestaurantDbContext context)
    {
        _context = context;
        Restaurants = new RestaurantRepository(context);
        Menus = new MenuRepository(context);
        Reviews = new ReviewRepository(context);
    }

    /// <inheritdoc/>
    public IRestaurantRepository Restaurants { get; }

    /// <inheritdoc/>
    public IMenuRepository Menus { get; }

    /// <inheritdoc/>
    public IReviewRepository Reviews { get; }

    /// <inheritdoc/>
    public async Task<int> SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}
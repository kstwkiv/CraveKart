using Restaurant.API.Application.Interfaces;

namespace Restaurant.API.Infrastructure.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly RestaurantDbContext _context;

    public UnitOfWork(RestaurantDbContext context)
    {
        _context = context;
        Restaurants = new RestaurantRepository(context);
        Menus = new MenuRepository(context);
        Reviews = new ReviewRepository(context);
    }

    public IRestaurantRepository Restaurants { get; }
    public IMenuRepository Menus { get; }
    public IReviewRepository Reviews { get; }

    public async Task<int> SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}
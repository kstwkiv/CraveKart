namespace Restaurant.API.Application.Interfaces;

public interface IUnitOfWork
{
    IRestaurantRepository Restaurants { get; }
    IMenuRepository Menus { get; }
    IReviewRepository Reviews { get; }
    Task<int> SaveChangesAsync();
}
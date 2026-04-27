using Restaurant.API.Domain.Entities;

namespace Restaurant.API.Application.Interfaces;

public interface IReviewRepository
{
    Task<IEnumerable<Review>> GetByRestaurantIdAsync(Guid restaurantId);
    Task<Review?> GetByOrderIdAsync(Guid orderId);
    Task<bool> ExistsForOrderAsync(Guid orderId, Guid customerId);
    Task AddAsync(Review review);
    Task<Review?> GetByIdAsync(Guid id);
    void Update(Review review);
    void Delete(Review review);
}

using Restaurant.API.Domain.Entities;

namespace Restaurant.API.Application.Interfaces;

/// <summary>
/// Repository interface for managing <see cref="Review"/> persistence operations.
/// </summary>
public interface IReviewRepository
{
    /// <summary>Retrieves all reviews for a specific restaurant, ordered by creation date descending.</summary>
    /// <param name="restaurantId">The unique identifier of the restaurant.</param>
    /// <returns>A collection of <see cref="Review"/> records for the restaurant.</returns>
    Task<IEnumerable<Review>> GetByRestaurantIdAsync(Guid restaurantId);

    /// <summary>Retrieves the review associated with a specific order.</summary>
    /// <param name="orderId">The unique identifier of the order.</param>
    /// <returns>The matching <see cref="Review"/>, or <c>null</c> if not found.</returns>
    Task<Review?> GetByOrderIdAsync(Guid orderId);

    /// <summary>Checks whether a customer has already reviewed a specific order.</summary>
    /// <param name="orderId">The unique identifier of the order.</param>
    /// <param name="customerId">The unique identifier of the customer.</param>
    /// <returns><c>true</c> if a review exists; otherwise <c>false</c>.</returns>
    Task<bool> ExistsForOrderAsync(Guid orderId, Guid customerId);

    /// <summary>Adds a new review to the repository.</summary>
    /// <param name="review">The <see cref="Review"/> to add.</param>
    Task AddAsync(Review review);

    /// <summary>Retrieves a review by its unique identifier.</summary>
    /// <param name="id">The unique identifier of the review.</param>
    /// <returns>The matching <see cref="Review"/>, or <c>null</c> if not found.</returns>
    Task<Review?> GetByIdAsync(Guid id);

    /// <summary>Marks an existing review as modified.</summary>
    /// <param name="review">The <see cref="Review"/> with updated values.</param>
    void Update(Review review);

    /// <summary>Removes a review from the repository.</summary>
    /// <param name="review">The <see cref="Review"/> to delete.</param>
    void Delete(Review review);
}

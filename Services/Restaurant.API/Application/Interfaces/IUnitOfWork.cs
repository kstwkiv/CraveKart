namespace Restaurant.API.Application.Interfaces;

/// <summary>
/// Unit of Work interface for the Restaurant bounded context.
/// Coordinates transactional access to restaurant, menu, and review repositories.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>Gets the repository for restaurant records.</summary>
    IRestaurantRepository Restaurants { get; }

    /// <summary>Gets the repository for menu categories and items.</summary>
    IMenuRepository Menus { get; }

    /// <summary>Gets the repository for customer reviews.</summary>
    IReviewRepository Reviews { get; }

    /// <summary>Persists all pending changes to the database.</summary>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync();
}
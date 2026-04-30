namespace Order.API.Application.Interfaces;

/// <summary>
/// Unit of Work interface for the Order bounded context.
/// Coordinates transactional access to order repositories.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>Gets the repository for order records.</summary>
    IOrderRepository Orders { get; }

    /// <summary>Persists all pending changes to the database.</summary>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync();
}
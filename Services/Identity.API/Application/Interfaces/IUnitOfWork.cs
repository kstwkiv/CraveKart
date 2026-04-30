namespace Identity.API.Application.Interfaces;

/// <summary>
/// Unit of Work interface for the Identity bounded context.
/// Coordinates transactional access to user repositories.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>Gets the repository for user accounts.</summary>
    IUserRepository Users { get; }

    /// <summary>Persists all pending changes to the database.</summary>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync();
}
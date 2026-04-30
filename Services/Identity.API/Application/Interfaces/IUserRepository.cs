using Identity.API.Domain.Entities;

namespace Identity.API.Application.Interfaces;

/// <summary>
/// Repository interface for managing <see cref="User"/> persistence operations.
/// </summary>
public interface IUserRepository
{
    /// <summary>Retrieves a user by their email address.</summary>
    /// <param name="email">The email address to search for.</param>
    /// <returns>The matching <see cref="User"/>, or <c>null</c> if not found.</returns>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>Retrieves a user by their unique identifier.</summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <returns>The matching <see cref="User"/>, or <c>null</c> if not found.</returns>
    Task<User?> GetByIdAsync(Guid id);

    /// <summary>Retrieves all user accounts ordered by creation date descending.</summary>
    /// <returns>A collection of all <see cref="User"/> records.</returns>
    Task<IEnumerable<User>> GetAllAsync();

    /// <summary>Checks whether an email address is already registered.</summary>
    /// <param name="email">The email address to check.</param>
    /// <returns><c>true</c> if the email is already in use; otherwise <c>false</c>.</returns>
    Task<bool> EmailExistsAsync(string email);

    /// <summary>Adds a new user account to the repository.</summary>
    /// <param name="user">The <see cref="User"/> to add.</param>
    Task AddAsync(User user);

    /// <summary>Marks an existing user account as modified.</summary>
    /// <param name="user">The <see cref="User"/> with updated values.</param>
    void Update(User user);
}
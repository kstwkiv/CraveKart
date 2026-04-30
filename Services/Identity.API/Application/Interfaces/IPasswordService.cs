namespace Identity.API.Application.Interfaces;

/// <summary>
/// Service interface for hashing and verifying passwords using BCrypt.
/// </summary>
public interface IPasswordService
{
    /// <summary>Hashes a plain-text password using BCrypt.</summary>
    /// <param name="password">The plain-text password to hash.</param>
    /// <returns>The BCrypt hash of the password.</returns>
    string HashPassword(string password);

    /// <summary>Verifies a plain-text password against a stored BCrypt hash.</summary>
    /// <param name="password">The plain-text password to verify.</param>
    /// <param name="hash">The stored BCrypt hash to compare against.</param>
    /// <returns><c>true</c> if the password matches the hash; otherwise <c>false</c>.</returns>
    bool VerifyPassword(string password, string hash);
}
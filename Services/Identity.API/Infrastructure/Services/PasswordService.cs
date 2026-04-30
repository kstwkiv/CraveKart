using Identity.API.Application.Interfaces;

namespace Identity.API.Infrastructure.Services;

/// <summary>
/// BCrypt-based implementation of <see cref="IPasswordService"/> for hashing and verifying passwords.
/// </summary>
public class PasswordService : IPasswordService
{
    /// <inheritdoc/>
    public string HashPassword(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password);

    /// <inheritdoc/>
    public bool VerifyPassword(string password, string hash) =>
        BCrypt.Net.BCrypt.Verify(password, hash);
}
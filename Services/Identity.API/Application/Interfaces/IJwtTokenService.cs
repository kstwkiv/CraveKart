using Identity.API.Domain.Entities;

namespace Identity.API.Application.Interfaces;

/// <summary>
/// Service interface for generating and validating JWT tokens.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>Generates a signed JWT access token for the specified user.</summary>
    /// <param name="user">The user for whom to generate the token.</param>
    /// <returns>A signed JWT token string.</returns>
    string GenerateToken(User user);

    /// <summary>Generates a cryptographically random refresh token.</summary>
    /// <returns>A Base64-encoded refresh token string.</returns>
    string GenerateRefreshToken();
}

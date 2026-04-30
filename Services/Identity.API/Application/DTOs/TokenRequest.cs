namespace Identity.API.Application.DTOs;

/// <summary>
/// Request DTO for refreshing an expired JWT access token using a valid refresh token.
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>Gets or sets the email address of the user requesting a token refresh.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the refresh token previously issued during login.</summary>
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// Request DTO for logging out a user and invalidating their refresh token.
/// </summary>
public class LogoutRequest
{
    /// <summary>Gets or sets the email address of the user to log out.</summary>
    public string Email { get; set; } = string.Empty;
}

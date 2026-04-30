namespace Identity.API.Application.DTOs;

/// <summary>
/// Response DTO returned after a successful login or registration.
/// Contains user identity information and authentication tokens.
/// </summary>
public class AuthResponse
{
    /// <summary>Gets or sets the unique identifier of the authenticated user.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the full name of the authenticated user.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Gets or sets the email address of the authenticated user.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the role of the authenticated user (e.g., "Customer", "Admin").</summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>Gets or sets the short-lived JWT access token for API authorization.</summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>Gets or sets the long-lived refresh token for obtaining new access tokens. Null on registration.</summary>
    public string? RefreshToken { get; set; }
}
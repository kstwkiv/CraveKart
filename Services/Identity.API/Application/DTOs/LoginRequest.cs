namespace Identity.API.Application.DTOs;

/// <summary>
/// Request DTO for authenticating an existing user.
/// </summary>
public class LoginRequest
{
    /// <summary>Gets or sets the user's registered email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the user's plain-text password.</summary>
    public string Password { get; set; } = string.Empty;
}
namespace Identity.API.Application.DTOs;

/// <summary>
/// Request DTO for registering a new user account.
/// </summary>
public class RegisterRequest
{
    /// <summary>Gets or sets the full name of the user.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Gets or sets the email address for the new account.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the plain-text password for the new account.</summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>Gets or sets the user's mobile phone number.</summary>
    public string MobileNumber { get; set; } = string.Empty;

    /// <summary>Gets or sets the role to assign to the new user. Defaults to "Customer".</summary>
    public string Role { get; set; } = "Customer";
}
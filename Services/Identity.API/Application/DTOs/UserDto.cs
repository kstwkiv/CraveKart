namespace Identity.API.Application.DTOs;

/// <summary>
/// Data transfer object representing a user account returned to API consumers.
/// </summary>
public class UserDto
{
    /// <summary>Gets or sets the unique identifier of the user.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the full name of the user.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Gets or sets the email address of the user.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the role of the user (e.g., "Customer", "Admin").</summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>Gets or sets the mobile phone number of the user.</summary>
    public string MobileNumber { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether the user account is active.</summary>
    public bool IsActive { get; set; }

    /// <summary>Gets or sets the UTC timestamp when the user account was created.</summary>
    public DateTime CreatedAt { get; set; }
}

using Identity.API.Domain.Enums;

namespace Identity.API.Domain.Entities;

/// <summary>
/// Represents a registered user account in the Identity bounded context.
/// </summary>
public class User
{
    /// <summary>Gets or sets the unique identifier of the user.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Gets or sets the full name of the user.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Gets or sets the email address of the user (unique).</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the BCrypt hash of the user's password.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>Gets or sets the user's mobile phone number.</summary>
    public string MobileNumber { get; set; } = string.Empty;

    /// <summary>Gets or sets the role assigned to the user.</summary>
    public UserRole Role { get; set; } = UserRole.Customer;

    /// <summary>Gets or sets a value indicating whether the user's email has been verified.</summary>
    public bool IsVerified { get; set; } = true;

    /// <summary>Gets or sets a value indicating whether the user account is active.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Gets or sets the BCrypt hash of the current refresh token. Null when logged out.</summary>
    public string? RefreshTokenHash { get; set; }

    /// <summary>Gets or sets the UTC expiry time of the current refresh token.</summary>
    public DateTime? RefreshTokenExpiry { get; set; }

    /// <summary>Gets or sets the BCrypt hash of the current password reset OTP.</summary>
    public string? PasswordResetOtp { get; set; }

    /// <summary>Gets or sets the UTC expiry time of the current password reset OTP.</summary>
    public DateTime? PasswordResetOtpExpiry { get; set; }

    /// <summary>Gets or sets the IST timestamp when the user account was created.</summary>
    public DateTime CreatedAt { get; set; } = IstClock.Now;

    /// <summary>Gets or sets the IST timestamp when the user account was last updated.</summary>
    public DateTime UpdatedAt { get; set; } = IstClock.Now;
}
namespace Identity.API.Application.DTOs;

/// <summary>
/// Request DTO for initiating a password reset by sending an OTP to the user's email.
/// </summary>
public class ForgotPasswordRequest
{
    /// <summary>Gets or sets the email address of the account to reset.</summary>
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Request DTO for completing a password reset using the OTP received via email.
/// </summary>
public class ResetPasswordRequest
{
    /// <summary>Gets or sets the email address of the account being reset.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the one-time password (OTP) sent to the user's email.</summary>
    public string Otp { get; set; } = string.Empty;

    /// <summary>Gets or sets the new plain-text password to set for the account.</summary>
    public string NewPassword { get; set; } = string.Empty;
}

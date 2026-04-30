using Identity.API.Application.Commands;
using Identity.API.Application.DTOs;
using Identity.API.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers;

/// <summary>
/// API controller for authentication operations including registration, login,
/// password reset, token refresh, and logout.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;
    private readonly IEmailService _emailService;

    /// <summary>
    /// Initializes a new instance of <see cref="AuthController"/>.
    /// </summary>
    /// <param name="authService">The authentication service for register and login operations.</param>
    /// <param name="unitOfWork">The unit of work for direct user data access.</param>
    /// <param name="passwordService">The password service for hashing and verification.</param>
    /// <param name="emailService">The email service for sending OTP emails.</param>
    public AuthController(
        IAuthService authService,
        IUnitOfWork unitOfWork,
        IPasswordService passwordService,
        IEmailService emailService)
    {
        _authService = authService;
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
        _emailService = emailService;
    }

    /// <summary>
    /// Registers a new user account and returns an access token.
    /// </summary>
    /// <param name="request">The registration request containing user details.</param>
    /// <returns>An <see cref="AuthResponse"/> with user info and access token.</returns>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var command = new RegisterUserCommand(
                request.FullName,
                request.Email,
                request.Password,
                request.MobileNumber,
                request.Role);

            var result = await _authService.RegisterAsync(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var command = new LoginCommand(request.Email, request.Password);
            var result = await _authService.LoginAsync(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    // POST /api/v1/auth/forgot-password
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);

        // Always return OK to prevent email enumeration
        if (user == null || !user.IsActive)
            return Ok(new { message = "If that email exists, an OTP has been sent." });

        var otp = new Random().Next(100000, 999999).ToString();
        user.PasswordResetOtp = _passwordService.HashPassword(otp);
        user.PasswordResetOtpExpiry = IstClock.Now.AddMinutes(15);
        user.UpdatedAt = IstClock.Now;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        await _emailService.SendAsync(
            user.Email,
            "CraveKart — Password Reset OTP 🔑",
            Identity.API.Infrastructure.Services.EmailTemplates.PasswordResetOtp(otp));

        return Ok(new { message = "If that email exists, an OTP has been sent." });
    }

    // POST /api/v1/auth/reset-password
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
        if (user == null || user.PasswordResetOtp == null || user.PasswordResetOtpExpiry == null)
            return BadRequest("Invalid or expired OTP.");

        if (user.PasswordResetOtpExpiry < IstClock.Now)
            return BadRequest("OTP has expired. Please request a new one.");

        if (!_passwordService.VerifyPassword(request.Otp, user.PasswordResetOtp))
            return BadRequest("Invalid OTP.");

        user.PasswordHash = _passwordService.HashPassword(request.NewPassword);
        user.PasswordResetOtp = null;
        user.PasswordResetOtpExpiry = null;
        user.UpdatedAt = IstClock.Now;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { message = "Password reset successfully." });
    }

    // POST /api/v1/auth/refresh-token
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
        if (user == null || user.RefreshTokenHash == null || user.RefreshTokenExpiry == null)
            return Unauthorized("Invalid refresh token.");

        if (user.RefreshTokenExpiry < IstClock.Now)
            return Unauthorized("Refresh token has expired. Please log in again.");

        if (!BCrypt.Net.BCrypt.Verify(request.RefreshToken, user.RefreshTokenHash))
            return Unauthorized("Invalid refresh token.");

        var jwtService = HttpContext.RequestServices.GetRequiredService<IJwtTokenService>();
        var newAccessToken = jwtService.GenerateToken(user);
        var newRefreshToken = jwtService.GenerateRefreshToken();

        user.RefreshTokenHash = BCrypt.Net.BCrypt.HashPassword(newRefreshToken);
        user.RefreshTokenExpiry = IstClock.Now.AddDays(7);
        user.UpdatedAt = IstClock.Now;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { accessToken = newAccessToken, refreshToken = newRefreshToken });
    }

    // POST /api/v1/auth/logout
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
        if (user != null)
        {
            user.RefreshTokenHash = null;
            user.RefreshTokenExpiry = null;
            user.UpdatedAt = IstClock.Now;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();
        }
        return Ok(new { message = "Logged out successfully." });
    }
}

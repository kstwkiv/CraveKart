// 'using' — imports the Commands namespace so RegisterUserCommand and LoginCommand are in scope
using Identity.API.Application.Commands;
// 'using' — imports the DTOs namespace (Data Transfer Objects that shape request/response payloads)
using Identity.API.Application.DTOs;
// 'using' — imports the Interfaces namespace so IAuthService, IUnitOfWork, etc. are available
using Identity.API.Application.Interfaces;
// 'using' — imports ASP.NET Core's authorisation primitives ([Authorize] attribute)
using Microsoft.AspNetCore.Authorization;
// 'using' — imports ASP.NET Core MVC types: ControllerBase, ActionResult, route attributes, etc.
using Microsoft.AspNetCore.Mvc;

// 'namespace' — logical container that groups the controller with other Identity API controllers
namespace Identity.API.Controllers;

/// <summary>
/// API controller for authentication operations including registration, login,
/// password reset, token refresh, and logout.
/// </summary>
// '[ApiController]' — attribute that enables automatic model validation, binding source inference, and problem-details error responses
// '[Route(...)]' — attribute that maps HTTP requests to this controller; [controller] is replaced by "auth" at runtime
[ApiController]
[Route("api/v1/[controller]")]
// 'public' — the controller class is visible to the ASP.NET Core routing infrastructure
// 'class' — reference type that groups related HTTP action methods
// ': ControllerBase' — inherits HTTP helpers (Ok, BadRequest, Unauthorized, etc.) without Razor view support
public class AuthController : ControllerBase
{
    // 'private' — field is encapsulated; only accessible within this class
    // 'readonly' — assigned once in the constructor; prevents accidental reassignment
    private readonly IAuthService _authService;
    // 'private readonly' — immutable reference to the unit-of-work for direct data access
    private readonly IUnitOfWork _unitOfWork;
    // 'private readonly' — immutable reference to the password hashing/verification service
    private readonly IPasswordService _passwordService;
    // 'private readonly' — immutable reference to the email sending service
    private readonly IEmailService _emailService;

    /// <summary>
    /// Initializes a new instance of <see cref="AuthController"/>.
    /// </summary>
    /// <param name="authService">The authentication service for register and login operations.</param>
    /// <param name="unitOfWork">The unit of work for direct user data access.</param>
    /// <param name="passwordService">The password service for hashing and verification.</param>
    /// <param name="emailService">The email service for sending OTP emails.</param>
    // 'public' — constructor is accessible to the ASP.NET Core DI container for injection
    public AuthController(
        IAuthService authService,
        IUnitOfWork unitOfWork,
        IPasswordService passwordService,
        IEmailService emailService)
    {
        // Stores each injected dependency in its corresponding readonly field
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
    // '[HttpPost]' — attribute that restricts this action to HTTP POST requests
    // '"register"' — appended to the controller route, making the full path POST /api/v1/auth/register
    [HttpPost("register")]
    // 'public' — action method must be public for the MVC routing engine to discover it
    // 'async' — method is asynchronous; the compiler generates a state machine to handle awaits
    // 'Task<T>' — wraps the eventual ActionResult so the caller can await the HTTP response
    // 'ActionResult<T>' — union type that can be either an HTTP result (Ok, BadRequest) or a typed value
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        // 'try' — begins a guarded block; any exception thrown inside is caught by the catch block
        try
        {
            // 'var' — implicitly typed local; compiler infers RegisterUserCommand from the right-hand side
            // 'new' — allocates a new RegisterUserCommand record/object on the heap
            var command = new RegisterUserCommand(
                request.FullName,
                request.Email,
                request.Password,
                request.MobileNumber,
                request.Role);

            // 'var' — type inferred as AuthResponse from the return type of RegisterAsync
            // 'await' — suspends execution until the async registration operation completes
            var result = await _authService.RegisterAsync(command);
            // 'return' — exits the method; Ok() wraps the result in an HTTP 200 response
            return Ok(result);
        }
        // 'catch' — intercepts any Exception thrown in the try block
        // 'Exception ex' — captures the thrown exception object for inspection
        catch (Exception ex)
        {
            // 'return' — exits with HTTP 400 Bad Request, including the exception message as the body
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Authenticates an existing user and returns access and refresh tokens.
    /// </summary>
    /// <param name="request">The login request containing email and password.</param>
    /// <returns>An <see cref="AuthResponse"/> with user info and tokens.</returns>
    // '[HttpPost("login")]' — maps POST /api/v1/auth/login to this action
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            // 'new' — creates a LoginCommand value object carrying the credentials
            var command = new LoginCommand(request.Email, request.Password);
            // 'await' — waits for the async login operation; result is an AuthResponse
            var result = await _authService.LoginAsync(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            // 'return' — HTTP 401 Unauthorized; used when credentials are invalid
            return Unauthorized(ex.Message);
        }
    }

    // POST /api/v1/auth/forgot-password
    /// <summary>
    /// Initiates a password reset by sending an OTP to the user's email.
    /// Always returns 200 OK to prevent email enumeration attacks.
    /// </summary>
    /// <param name="request">The request containing the user's email address.</param>
    /// <returns>200 OK with a generic confirmation message.</returns>
    // '[HttpPost]' — restricts to POST; sensitive operations should never be GET (no caching/logging of secrets)
    [HttpPost("forgot-password")]
    // 'IActionResult' — non-generic interface; allows returning any HTTP status without a typed body
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        // 'await' — waits for the async DB lookup before proceeding
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);

        // Always return OK to prevent email enumeration
        // 'if' — conditional: returns early with a generic message regardless of whether the user exists
        // 'null' — user not found in the database
        // '!' — logical NOT; checks that the user is not active
        if (user == null || !user.IsActive)
            return Ok(new { message = "If that email exists, an OTP has been sent." });

        // 'var' — inferred as string; generates a 6-digit OTP using Random
        // 'new Random()' — creates a pseudo-random number generator instance
        var otp = new Random().Next(100000, 999999).ToString();
        // Hashes the OTP before storing it (never store secrets in plain text)
        user.PasswordResetOtp = _passwordService.HashPassword(otp);
        // Sets a 15-minute expiry window for the OTP
        user.PasswordResetOtpExpiry = IstClock.Now.AddMinutes(15);
        user.UpdatedAt = IstClock.Now;
        _unitOfWork.Users.Update(user);
        // 'await' — persists the OTP hash and expiry to the database asynchronously
        await _unitOfWork.SaveChangesAsync();

        // 'await' — waits for the email to be dispatched before returning the response
        await _emailService.SendAsync(
            user.Email,
            "CraveKart — Password Reset OTP 🔑",
            Identity.API.Infrastructure.Services.EmailTemplates.PasswordResetOtp(otp));

        return Ok(new { message = "If that email exists, an OTP has been sent." });
    }

    // POST /api/v1/auth/reset-password
    /// <summary>
    /// Resets the user's password using a valid OTP.
    /// </summary>
    /// <param name="request">The request containing the email, OTP, and new password.</param>
    /// <returns>200 OK on success, or 400 if the OTP is invalid or expired.</returns>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
        // 'if' — guard clause: rejects the request if the user or OTP data is missing
        // 'null' — absence of a value; OTP fields are null when no reset was requested
        if (user == null || user.PasswordResetOtp == null || user.PasswordResetOtpExpiry == null)
            return BadRequest("Invalid or expired OTP.");

        // 'if' — checks whether the OTP window has passed; '<' compares two DateTime values
        if (user.PasswordResetOtpExpiry < IstClock.Now)
            return BadRequest("OTP has expired. Please request a new one.");

        // 'if' — verifies the submitted OTP against the stored hash; '!' negates the result
        if (!_passwordService.VerifyPassword(request.Otp, user.PasswordResetOtp))
            return BadRequest("Invalid OTP.");

        // Replaces the old password hash with a hash of the new password
        user.PasswordHash = _passwordService.HashPassword(request.NewPassword);
        // 'null' — clears the OTP fields so the same OTP cannot be reused
        user.PasswordResetOtp = null;
        user.PasswordResetOtpExpiry = null;
        user.UpdatedAt = IstClock.Now;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { message = "Password reset successfully." });
    }

    // POST /api/v1/auth/refresh-token
    /// <summary>
    /// Issues a new access token and refresh token using a valid refresh token.
    /// </summary>
    /// <param name="request">The request containing the user's email and current refresh token.</param>
    /// <returns>New access and refresh tokens, or 401 if the refresh token is invalid or expired.</returns>
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
        // 'if' — guard: rejects if user not found or no refresh token is stored
        // 'null' — indicates no active refresh token session exists
        if (user == null || user.RefreshTokenHash == null || user.RefreshTokenExpiry == null)
            return Unauthorized("Invalid refresh token.");

        // 'if' — checks token expiry; expired tokens must not be honoured (security requirement)
        if (user.RefreshTokenExpiry < IstClock.Now)
            return Unauthorized("Refresh token has expired. Please log in again.");

        // 'if' — verifies the submitted token against the stored BCrypt hash
        if (!BCrypt.Net.BCrypt.Verify(request.RefreshToken, user.RefreshTokenHash))
            return Unauthorized("Invalid refresh token.");

        // 'var' — resolves IJwtTokenService from the DI container at runtime (service-locator pattern)
        var jwtService = HttpContext.RequestServices.GetRequiredService<IJwtTokenService>();
        // 'var' — inferred as string; a new short-lived JWT access token
        var newAccessToken = jwtService.GenerateToken(user);
        // 'var' — inferred as string; a new opaque refresh token (random bytes, base64-encoded)
        var newRefreshToken = jwtService.GenerateRefreshToken();

        // Stores a BCrypt hash of the new refresh token (never store tokens in plain text)
        user.RefreshTokenHash = BCrypt.Net.BCrypt.HashPassword(newRefreshToken);
        // Sets a 7-day sliding expiry window for the new refresh token
        user.RefreshTokenExpiry = IstClock.Now.AddDays(7);
        user.UpdatedAt = IstClock.Now;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        // 'return' — HTTP 200 with both tokens in an anonymous object payload
        // 'new' — creates an anonymous type; compiler generates a class with the specified properties
        return Ok(new { accessToken = newAccessToken, refreshToken = newRefreshToken });
    }

    // POST /api/v1/auth/logout
    /// <summary>
    /// Logs out the authenticated user by invalidating their refresh token.
    /// </summary>
    /// <param name="request">The request containing the user's email.</param>
    /// <returns>200 OK with a confirmation message.</returns>
    [HttpPost("logout")]
    // '[Authorize]' — attribute that enforces authentication; only requests with a valid JWT can reach this action
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
        // 'if' — only clears tokens if the user exists; avoids a null reference exception
        // 'null' — checked to guard against a missing user record
        if (user != null)
        {
            // 'null' — sets the refresh token fields to null, effectively invalidating the session
            user.RefreshTokenHash = null;
            user.RefreshTokenExpiry = null;
            user.UpdatedAt = IstClock.Now;
            _unitOfWork.Users.Update(user);
            // 'await' — waits for the database write to complete before returning the response
            await _unitOfWork.SaveChangesAsync();
        }
        // 'return' — HTTP 200 confirms the logout regardless of whether a token was found
        return Ok(new { message = "Logged out successfully." });
    }
}

using Identity.API.Application.Commands;
using Identity.API.Application.DTOs;
using Identity.API.Application.Interfaces;
using MediatR;


// <LoginHandler> is handling a login request using MediatR (CQRS pattern)

namespace Identity.API.Application.Handlers;

/// <summary>
/// MediatR handler that processes <see cref="LoginCommand"/> requests.
/// Validates credentials, generates JWT and refresh tokens, and persists the refresh token hash.
/// </summary>
public class LoginHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordService _passwordService;

    /// <summary>
    /// Initializes a new instance of <see cref="LoginHandler"/>.
    /// </summary>
    /// <param name="unitOfWork">The unit of work for data access.</param>
    /// <param name="jwtTokenService">The JWT token service for generating tokens.</param>
    /// <param name="passwordService">The password service for verifying credentials.</param>
    public LoginHandler(
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService,
        IPasswordService passwordService)
    {
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _passwordService = passwordService;
    }

    /// <summary>
    /// Handles the login request by verifying credentials and issuing authentication tokens.
    /// </summary>
    /// <param name="request">The login command containing email and password.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>An <see cref="AuthResponse"/> with user info and tokens.</returns>
    /// <exception cref="Exception">Thrown when credentials are invalid or the account is deactivated.</exception>
    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);

        if (user == null || !_passwordService.VerifyPassword(request.Password, user.PasswordHash))
            throw new Exception("Invalid email or password.");

        if (!user.IsActive)
            throw new Exception("Account is deactivated.");

        var token = _jwtTokenService.GenerateToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        user.RefreshTokenHash = BCrypt.Net.BCrypt.HashPassword(refreshToken);
        user.RefreshTokenExpiry = IstClock.Now.AddDays(7);
        user.UpdatedAt = IstClock.Now;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return new AuthResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            AccessToken = token,
            RefreshToken = refreshToken
        };
    }
}

//// MediatR : Decouples controller from business logic
/// Validates user → generates JWT + refresh token → stores secure refresh token → returns authentication response
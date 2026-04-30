using FoodFleet.Shared.Events.Auth;
using FoodFleet.Shared.Messaging.Interfaces;
using Identity.API.Application.Commands;
using Identity.API.Application.DTOs;
using Identity.API.Application.Interfaces;
using Identity.API.Domain.Entities;
using Identity.API.Domain.Enums;

namespace Identity.API.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordService _passwordService;
    private readonly IEventPublisher _eventPublisher;

    public AuthService(
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService,
        IPasswordService passwordService,
        IEventPublisher eventPublisher)
    {
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _passwordService = passwordService;
        _eventPublisher = eventPublisher;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterUserCommand request, CancellationToken cancellationToken = default)
    {
        var emailExists = await _unitOfWork.Users.EmailExistsAsync(request.Email);
        if (emailExists)
            throw new Exception("Email already registered.");

        if (!Enum.TryParse<UserRole>(request.Role, ignoreCase: true, out var role))
            role = UserRole.Customer;

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = _passwordService.HashPassword(request.Password),
            MobileNumber = request.MobileNumber,
            Role = role,
            IsVerified = true
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Publish with a fresh token so the HTTP cancellation doesn't abort the event
        await _eventPublisher.PublishAsync(new UserRegisteredEvent
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            RegisteredAt = IstClock.Now
        }, CancellationToken.None);

        return new AuthResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            AccessToken = _jwtTokenService.GenerateToken(user)
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginCommand request, CancellationToken cancellationToken = default)
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

        await _eventPublisher.PublishAsync(new UserLoggedInEvent
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            LoggedInAt = IstClock.Now
        }, CancellationToken.None);

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

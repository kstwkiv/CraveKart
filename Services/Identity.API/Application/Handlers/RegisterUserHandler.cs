using FoodFleet.Shared.Events.Auth;
using FoodFleet.Shared.Messaging.Interfaces;
using Identity.API.Application.Commands;
using Identity.API.Application.DTOs;
using Identity.API.Application.Interfaces;
using Identity.API.Domain.Entities;
using Identity.API.Domain.Enums;
using MediatR;

//// Creates user + secures password + publishes event + returns JWT

namespace Identity.API.Application.Handlers;

/// <summary>
/// MediatR handler that processes <see cref="RegisterUserCommand"/> requests.
/// Creates a new user account, hashes the password, and publishes a <see cref="UserRegisteredEvent"/>.
/// </summary>
public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, AuthResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordService _passwordService;
    private readonly IEventPublisher _eventPublisher;

    /// <summary>
    /// Initializes a new instance of <see cref="RegisterUserHandler"/>.
    /// </summary>
    /// <param name="unitOfWork">The unit of work for data access.</param>
    /// <param name="jwtTokenService">The JWT token service for generating access tokens.</param>
    /// <param name="passwordService">The password service for hashing passwords.</param>
    /// <param name="eventPublisher">The event publisher for raising domain events.</param>
    public RegisterUserHandler(
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

    /// <summary>
    /// Handles the user registration request, creates the account, and returns an access token.
    /// </summary>
    /// <param name="request">The registration command with user details.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>An <see cref="AuthResponse"/> with user info and access token.</returns>
    /// <exception cref="Exception">Thrown when the email address is already registered.</exception>
    public async Task<AuthResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
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

        await _eventPublisher.PublishAsync(new UserRegisteredEvent
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            RegisteredAt = IstClock.Now
        }, cancellationToken);

        var token = _jwtTokenService.GenerateToken(user);

        return new AuthResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            AccessToken = token
        };
    }
}
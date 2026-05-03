// 'using' — imports the shared events namespace so UserRegisteredEvent is available without full qualification
using FoodFleet.Shared.Events.Auth;
// 'using' — imports the messaging interfaces namespace (IEventPublisher) for publishing domain events
using FoodFleet.Shared.Messaging.Interfaces;
// 'using' — brings in the Commands namespace containing RegisterUserCommand
using Identity.API.Application.Commands;
// 'using' — imports DTOs (Data Transfer Objects) like AuthResponse used as the return type
using Identity.API.Application.DTOs;
// 'using' — imports application-level interfaces (IUnitOfWork, IJwtTokenService, IPasswordService)
using Identity.API.Application.Interfaces;
// 'using' — imports the domain entity User from the Domain layer
using Identity.API.Domain.Entities;
// 'using' — imports the UserRole enum from the Domain layer
using Identity.API.Domain.Enums;
// 'using' — imports MediatR, the library implementing the Mediator / CQRS pattern
using MediatR;

//// Creates user + secures password + publishes event + returns JWT

// 'namespace' — declares a logical container for this class, preventing naming conflicts across the solution
namespace Identity.API.Application.Handlers;

/// <summary>
/// MediatR handler that processes <see cref="RegisterUserCommand"/> requests.
/// Creates a new user account, hashes the password, and publishes a <see cref="UserRegisteredEvent"/>.
/// </summary>
// 'public' — access modifier: class is visible to all other assemblies (required for DI registration)
// 'class' — defines a reference type; a blueprint for objects that handle user registration
// 'IRequestHandler<TRequest, TResponse>' — MediatR interface contract; mandates implementing Handle()
public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, AuthResponse>
{
    // 'private' — encapsulation: field hidden from outside the class
    // 'readonly' — field is assigned once (in constructor) and never changed; promotes immutability
    private readonly IUnitOfWork _unitOfWork;
    // 'private readonly' — JWT service injected via constructor; readonly prevents accidental reassignment
    private readonly IJwtTokenService _jwtTokenService;
    // 'private readonly' — password hashing/verification service; immutable after construction
    private readonly IPasswordService _passwordService;
    // 'private readonly' — event publisher for broadcasting domain events to the message bus
    private readonly IEventPublisher _eventPublisher;

    /// <summary>
    /// Initializes a new instance of <see cref="RegisterUserHandler"/>.
    /// </summary>
    /// <param name="unitOfWork">The unit of work for data access.</param>
    /// <param name="jwtTokenService">The JWT token service for generating access tokens.</param>
    /// <param name="passwordService">The password service for hashing passwords.</param>
    /// <param name="eventPublisher">The event publisher for raising domain events.</param>
    // 'public' — constructor visibility required so the DI container can resolve and inject dependencies
    public RegisterUserHandler(
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService,
        IPasswordService passwordService,
        IEventPublisher eventPublisher)
    {
        // Constructor injection — assigns all dependencies to readonly fields (Dependency Inversion Principle)
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
    // 'public' — satisfies the IRequestHandler interface; must be accessible to MediatR's dispatcher
    // 'async' — marks the method as asynchronous; the compiler generates a state machine under the hood
    // 'Task<AuthResponse>' — represents an in-progress operation that will eventually yield an AuthResponse
    public async Task<AuthResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // 'var' — implicitly typed; compiler infers bool from EmailExistsAsync return type
        // 'await' — suspends this method (without blocking the thread) until the async check completes
        var emailExists = await _unitOfWork.Users.EmailExistsAsync(request.Email);
        // 'if' — conditional guard: prevents duplicate registrations
        // 'throw' — raises an exception to signal a business rule violation to the caller
        // 'new' — allocates a new Exception object on the heap
        if (emailExists)
            throw new Exception("Email already registered.");

        // 'if' — conditional: tries to parse the role string; falls back to Customer if parsing fails
        // 'var' — compiler infers UserRole for the out variable
        // 'out' — passes the variable by reference so TryParse can write the parsed value into it
        if (!Enum.TryParse<UserRole>(request.Role, ignoreCase: true, out var role))
            // Assignment inside else branch — default role when the provided string is unrecognised
            role = UserRole.Customer;

        // 'var' — compiler infers type User from the right-hand side object initializer
        // 'new' — allocates a new User entity on the managed heap
        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            // Hashing the password before persisting — never store plain-text passwords
            PasswordHash = _passwordService.HashPassword(request.Password),
            MobileNumber = request.MobileNumber,
            Role = role,
            IsVerified = true
        };

        // 'await' — asynchronously adds the user entity to the repository's change tracker
        await _unitOfWork.Users.AddAsync(user);
        // 'await' — asynchronously flushes all tracked changes to the database in a single transaction
        await _unitOfWork.SaveChangesAsync();

        // 'await' — asynchronously publishes a domain event to the message bus (e.g. RabbitMQ)
        // 'new' — creates a new UserRegisteredEvent value to carry the event payload
        await _eventPublisher.PublishAsync(new UserRegisteredEvent
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            RegisteredAt = IstClock.Now
        }, cancellationToken);

        // 'var' — compiler infers string for the JWT access token
        var token = _jwtTokenService.GenerateToken(user);

        // 'return' — exits the method and resolves the Task<AuthResponse> with the constructed DTO
        // 'new' — allocates the AuthResponse DTO that is sent back to the API controller
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

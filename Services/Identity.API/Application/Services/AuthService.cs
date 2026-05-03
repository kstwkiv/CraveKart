// 'using' — imports a namespace so its types are available without full qualification
using FoodFleet.Shared.Events.Auth;
// 'using' — brings in the shared messaging abstraction namespace
using FoodFleet.Shared.Messaging.Interfaces;
// 'using' — imports the Commands namespace containing RegisterUserCommand / LoginCommand
using Identity.API.Application.Commands;
// 'using' — imports DTOs (Data Transfer Objects) used as return types
using Identity.API.Application.DTOs;
// 'using' — imports the IAuthService interface this class implements
using Identity.API.Application.Interfaces;
// 'using' — imports the User domain entity
using Identity.API.Domain.Entities;
// 'using' — imports the UserRole enum defined in the domain layer
using Identity.API.Domain.Enums;

// 'namespace' — declares a logical grouping/scope for this class, preventing name collisions
namespace Identity.API.Application.Services;

/// <summary>
/// Application service implementing <see cref="IAuthService"/> for core authentication operations.
/// Handles user registration and login, hashing passwords, generating JWT tokens,
/// and publishing domain events via <see cref="IEventPublisher"/>.
/// </summary>
// 'public' — access modifier: this class is visible to any other assembly or namespace
// 'class' — defines a reference type that encapsulates data and behaviour
public class AuthService : IAuthService // ':' means AuthService inherits/implements IAuthService (interface contract)
{
    // 'private' — access modifier: field is only accessible within this class
    // 'readonly' — the field can only be assigned in the constructor; prevents accidental reassignment
    private readonly IUnitOfWork _unitOfWork;

    // 'private readonly' — same as above; IJwtTokenService is an interface (abstraction over JWT logic)
    private readonly IJwtTokenService _jwtTokenService;

    // 'private readonly' — IPasswordService abstracts hashing/verification so the algorithm can be swapped
    private readonly IPasswordService _passwordService;

    // 'private readonly' — IEventPublisher abstracts the message-bus so the transport can be swapped
    private readonly IEventPublisher _eventPublisher;

    /// <summary>
    /// Initializes a new instance of <see cref="AuthService"/>.
    /// </summary>
    /// <param name="unitOfWork">The unit of work for data access.</param>
    /// <param name="jwtTokenService">The JWT token service for generating access tokens.</param>
    /// <param name="passwordService">The password service for hashing and verifying passwords.</param>
    /// <param name="eventPublisher">The event publisher for raising domain events.</param>
    // 'public' — constructor is accessible so the DI container can instantiate this class
    public AuthService(
        IUnitOfWork unitOfWork,           // Dependency Injection: the runtime supplies the concrete implementation
        IJwtTokenService jwtTokenService, // Dependency Injection: decouples JWT generation from this class
        IPasswordService passwordService, // Dependency Injection: decouples password hashing from this class
        IEventPublisher eventPublisher)   // Dependency Injection: decouples event publishing from this class
    {
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _passwordService = passwordService;
        _eventPublisher = eventPublisher;
    }

    // 'public' — method is part of the IAuthService contract, so it must be accessible
    // 'async' — marks the method as asynchronous; the compiler rewrites it as a state machine
    // 'Task<AuthResponse>' — Task represents an in-flight async operation; <AuthResponse> is the eventual result type
    public async Task<AuthResponse> RegisterAsync(RegisterUserCommand request, CancellationToken cancellationToken = default)
    {
        // 'var' — implicitly typed local variable; the compiler infers the type (bool) from the right-hand side
        // 'await' — suspends execution of this method until the awaited Task completes, freeing the thread
        var emailExists = await _unitOfWork.Users.EmailExistsAsync(request.Email);
        // 'if' — conditional branch: executes the block only when the condition is true
        if (emailExists)
            // 'throw' — raises an exception, unwinding the call stack to the nearest matching catch block
            throw new Exception("Email already registered.");

        // 'if' — checks whether the role string can be parsed into the UserRole enum
        // 'out' — output parameter: Enum.TryParse writes the parsed value into 'role' without a return value
        if (!Enum.TryParse<UserRole>(request.Role, ignoreCase: true, out var role))
            role = UserRole.Customer; // fallback to the default enum member

        // 'var' — compiler infers type User; 'new' allocates a fresh User object on the managed heap
        // 'new' — creates a new instance of the User class, calling its constructor
        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = _passwordService.HashPassword(request.Password), // store only the hash, never plain text
            MobileNumber = request.MobileNumber,
            Role = role,
            IsVerified = true // 'true' — boolean literal representing the logical value "yes"
        };

        // 'await' — asynchronously waits for the user to be persisted to the database
        await _unitOfWork.Users.AddAsync(user);
        // 'await' — flushes all pending changes in the unit-of-work to the database atomically
        await _unitOfWork.SaveChangesAsync();

        // Publish with a fresh token so the HTTP cancellation doesn't abort the event
        // 'await' — asynchronously publishes the domain event to the message bus
        // 'new' — allocates a new UserRegisteredEvent record/object
        await _eventPublisher.PublishAsync(new UserRegisteredEvent
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(), // ToString() converts the enum member to its string name
            RegisteredAt = IstClock.Now
        }, CancellationToken.None); // CancellationToken.None — a token that can never be cancelled

        // 'return' — exits the method and hands the value back to the caller
        // 'new' — allocates the AuthResponse DTO that carries the result back through the HTTP layer
        return new AuthResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            AccessToken = _jwtTokenService.GenerateToken(user) // JWT: a signed, self-contained bearer token
        };
    }

    // 'public async Task<AuthResponse>' — same async pattern as RegisterAsync; returns a login result
    public async Task<AuthResponse> LoginAsync(LoginCommand request, CancellationToken cancellationToken = default)
    {
        // 'var' — compiler infers type User? (nullable); null means no matching user was found
        // 'await' — suspends until the database query completes
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);

        // 'if' — guards against null user or wrong password in a single branch (short-circuit evaluation)
        // 'null' — the absence of an object reference; checking for null prevents NullReferenceException
        if (user == null || !_passwordService.VerifyPassword(request.Password, user.PasswordHash))
            // 'throw' — raises an exception; intentionally vague message to prevent user enumeration
            throw new Exception("Invalid email or password.");

        // 'if' — checks a boolean property; '!' negates it
        if (!user.IsActive)
            throw new Exception("Account is deactivated.");

        // 'var' — compiler infers string; GenerateToken produces a signed JWT
        var token = _jwtTokenService.GenerateToken(user);
        // 'var' — compiler infers string; GenerateRefreshToken produces a long-lived opaque token
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        // Store only the hash of the refresh token — never the raw value — for security
        user.RefreshTokenHash = BCrypt.Net.BCrypt.HashPassword(refreshToken);
        user.RefreshTokenExpiry = IstClock.Now.AddDays(7); // AddDays — extends the expiry by 7 days
        user.UpdatedAt = IstClock.Now;
        _unitOfWork.Users.Update(user); // marks the entity as modified in the change tracker
        // 'await' — persists the refresh-token hash to the database
        await _unitOfWork.SaveChangesAsync();

        // 'await' — publishes the login event asynchronously; CancellationToken.None prevents premature abort
        await _eventPublisher.PublishAsync(new UserLoggedInEvent
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            LoggedInAt = IstClock.Now
        }, CancellationToken.None);

        // 'return' — sends the populated AuthResponse DTO back to the calling handler/controller
        return new AuthResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            AccessToken = token,       // short-lived JWT for API authorization
            RefreshToken = refreshToken // long-lived token used to obtain new JWTs without re-login
        };
    }
}

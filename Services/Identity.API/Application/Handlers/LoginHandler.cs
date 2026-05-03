// 'using' — imports a namespace so its types are available without full qualification
using Identity.API.Application.Commands;
// 'using' — brings in the DTOs namespace containing data transfer objects like AuthResponse
using Identity.API.Application.DTOs;
// 'using' — imports the Interfaces namespace so IUnitOfWork, IJwtTokenService, etc. are accessible
using Identity.API.Application.Interfaces;
// 'using' — imports MediatR, the library that implements the Mediator pattern (decouples senders from handlers)
using MediatR;


// <LoginHandler> is handling a login request using MediatR (CQRS pattern)

// 'namespace' — declares a logical grouping/scope for this class, preventing name collisions across assemblies
namespace Identity.API.Application.Handlers;

/// <summary>
/// MediatR handler that processes <see cref="LoginCommand"/> requests.
/// Validates credentials, generates JWT and refresh tokens, and persists the refresh token hash.
/// </summary>
// 'public' — access modifier: this class is visible to any other code in any assembly
// 'class' — defines a reference type that encapsulates data and behavior (object-oriented blueprint)
// 'IRequestHandler<TRequest, TResponse>' — interface contract from MediatR; forces this class to implement Handle()
public class LoginHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    // 'private' — access modifier: field is only accessible within this class (encapsulation principle)
    // 'readonly' — the field can only be assigned during declaration or in the constructor (immutability guarantee)
    private readonly IUnitOfWork _unitOfWork;
    // 'private readonly' — same immutability + encapsulation; injected once via constructor, never reassigned
    private readonly IJwtTokenService _jwtTokenService;
    // 'private readonly' — dependency injected password service; readonly prevents accidental reassignment
    private readonly IPasswordService _passwordService;

    /// <summary>
    /// Initializes a new instance of <see cref="LoginHandler"/>.
    /// </summary>
    /// <param name="unitOfWork">The unit of work for data access.</param>
    /// <param name="jwtTokenService">The JWT token service for generating tokens.</param>
    /// <param name="passwordService">The password service for verifying credentials.</param>
    // 'public' — constructor is accessible so the DI container can instantiate this handler
    public LoginHandler(
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService,
        IPasswordService passwordService)
    {
        // Assigning constructor parameters to readonly fields — standard constructor injection (Dependency Inversion Principle)
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
    // 'public' — must be public to satisfy the IRequestHandler interface contract
    // 'async' — marks the method as asynchronous; the compiler rewrites it as a state machine
    // 'Task<AuthResponse>' — Task is the .NET representation of a future value; <AuthResponse> is the eventual result type
    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // 'var' — implicitly typed local variable; the compiler infers the type (User) from the right-hand side
        // 'await' — suspends execution of this method until the awaited Task completes, freeing the thread in the meantime
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);

        // 'if' — conditional branching: executes the following statement only when the condition is true
        // 'null' — represents the absence of an object reference
        // 'throw' — raises an exception, unwinding the call stack to the nearest matching catch block
        // 'new' — allocates a new object instance on the managed heap
        if (user == null || !_passwordService.VerifyPassword(request.Password, user.PasswordHash))
            throw new Exception("Invalid email or password.");

        // 'if' — another conditional guard; checks a boolean property before proceeding
        // 'throw' — terminates normal flow and signals an error condition to the caller
        if (!user.IsActive)
            throw new Exception("Account is deactivated.");

        // 'var' — compiler infers type string for the JWT access token
        var token = _jwtTokenService.GenerateToken(user);
        // 'var' — compiler infers type string for the opaque refresh token
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        // Updating entity state — the Unit of Work pattern tracks these changes until SaveChangesAsync is called
        user.RefreshTokenHash = BCrypt.Net.BCrypt.HashPassword(refreshToken);
        user.RefreshTokenExpiry = IstClock.Now.AddDays(7);
        user.UpdatedAt = IstClock.Now;
        _unitOfWork.Users.Update(user);
        // 'await' — asynchronously waits for all pending changes to be persisted to the database
        await _unitOfWork.SaveChangesAsync();

        // 'return' — exits the method and hands the specified value back to the caller (or completes the Task)
        // 'new' — creates a new AuthResponse object using object-initializer syntax
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

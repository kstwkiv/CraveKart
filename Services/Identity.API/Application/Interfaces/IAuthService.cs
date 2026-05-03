// 'using' — imports the Commands namespace so RegisterUserCommand and LoginCommand are in scope
using Identity.API.Application.Commands;
// 'using' — imports the DTOs namespace so AuthResponse (the return type) is available
using Identity.API.Application.DTOs;

// 'namespace' — logical grouping that scopes this interface to the Interfaces layer, avoiding name collisions
namespace Identity.API.Application.Interfaces;

/// <summary>
/// Service interface for core authentication operations: registration and login.
/// </summary>
/// 
/// Any class that implements this interface must provide Register and Login functionality.
// 'public' — access modifier: the interface is visible to all assemblies (needed for DI and implementations)
// 'interface' — defines a pure contract (no implementation); any class that implements it must provide all members
//               Interfaces enable the Dependency Inversion Principle: depend on abstractions, not concretions
public interface IAuthService
{
    /// <summary>Registers a new user account and returns authentication tokens.</summary>
    /// <param name="request">The command containing user registration details.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>An <see cref="AuthResponse"/> with user info and access token.</returns>
    
    // 'Task<AuthResponse>' — the method is asynchronous by convention; callers must await it
    //                        Task represents a future value; <AuthResponse> is the eventual result type
    // 'default' — provides a default value for the optional CancellationToken parameter
    Task<AuthResponse> RegisterAsync(RegisterUserCommand request, CancellationToken cancellationToken = default);


    /// So your controllers don't depend directly on MediatR or handlers

    /// <summary>Authenticates an existing user and returns authentication tokens.</summary>
    /// <param name="request">The command containing login credentials.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>An <see cref="AuthResponse"/> with user info and access/refresh tokens.</returns>
    // 'Task<AuthResponse>' — same async contract; returns a future AuthResponse when login completes
    // 'default' — makes CancellationToken optional so callers that don't need cancellation can omit it
    Task<AuthResponse> LoginAsync(LoginCommand request, CancellationToken cancellationToken = default);
}

using Identity.API.Application.Commands;
using Identity.API.Application.DTOs;

namespace Identity.API.Application.Interfaces;

/// <summary>
/// Service interface for core authentication operations: registration and login.
/// </summary>
/// 
/// Any class that implements this interface must provide Register and Login functionality.
public interface IAuthService
{
    /// <summary>Registers a new user account and returns authentication tokens.</summary>
    /// <param name="request">The command containing user registration details.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>An <see cref="AuthResponse"/> with user info and access token.</returns>
    
    Task<AuthResponse> RegisterAsync(RegisterUserCommand request, CancellationToken cancellationToken = default);


    /// So your controllers don’t depend directly on MediatR or handlers

    /// <summary>Authenticates an existing user and returns authentication tokens.</summary>
    /// <param name="request">The command containing login credentials.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>An <see cref="AuthResponse"/> with user info and access/refresh tokens.</returns>
    Task<AuthResponse> LoginAsync(LoginCommand request, CancellationToken cancellationToken = default);
}

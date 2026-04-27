using Identity.API.Application.Commands;
using Identity.API.Application.DTOs;

namespace Identity.API.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterUserCommand request, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginAsync(LoginCommand request, CancellationToken cancellationToken = default);
}

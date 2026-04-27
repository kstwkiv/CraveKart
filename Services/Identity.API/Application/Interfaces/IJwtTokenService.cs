using Identity.API.Domain.Entities;

namespace Identity.API.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
    string GenerateRefreshToken();
}

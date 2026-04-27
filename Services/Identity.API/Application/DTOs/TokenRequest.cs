namespace Identity.API.Application.DTOs;

public class RefreshTokenRequest
{
    public string Email { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

public class LogoutRequest
{
    public string Email { get; set; } = string.Empty;
}

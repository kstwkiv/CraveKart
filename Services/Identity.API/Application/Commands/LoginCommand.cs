using Identity.API.Application.DTOs;

namespace Identity.API.Application.Commands;

public record LoginCommand(
    string Email,
    string Password);
